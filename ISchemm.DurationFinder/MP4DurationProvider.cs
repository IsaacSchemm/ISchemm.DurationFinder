using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class MP4DurationProvider : IDurationProvider {
        private static unsafe string ToFourCharacterString(int val) {
            int* ptr1 = &val;
            sbyte* ptr2 = (sbyte*)ptr1;
            return new string(ptr2, 0, sizeof(int));
        }

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        private unsafe struct BigEndianUInt32 {
            private readonly int val;
            public static implicit operator uint (BigEndianUInt32 x) => (uint)IPAddress.NetworkToHostOrder(x.val);
            public override string ToString() => $"{(uint)this}";
        }

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        private unsafe struct AtomType {
            private readonly int val;
            public static unsafe implicit operator string(AtomType x) => ToFourCharacterString(x.val);
            public override string ToString() => (string)this;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AtomHeader {
            public readonly BigEndianUInt32 length;
            public readonly AtomType type;

            public static unsafe int Size => sizeof(AtomHeader);

            public static unsafe AtomHeader FromByteArray(byte[] arr) {
                fixed (byte* ptr = arr) {
                    var ptr2 = (AtomHeader*)ptr;
                    return *ptr2;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MovieHeaderAtom {
            private readonly AtomHeader header;
            private readonly byte version;
            private readonly byte flags1;
            private readonly byte flags2;
            private readonly byte flags3;
            private readonly BigEndianUInt32 creationTime;
            private readonly BigEndianUInt32 modificationTime;
            private readonly BigEndianUInt32 timeScale;
            private readonly BigEndianUInt32 duration;

            public DateTime CreationTime => new DateTime(1904, 1, 1, 0, 0, 0).AddSeconds(creationTime);
            public DateTime ModificationTime => new DateTime(1904, 1, 1, 0, 0, 0).AddSeconds(modificationTime);

            public TimeSpan Duration {
                get {
                    double t = timeScale;
                    double d = duration;
                    return TimeSpan.FromSeconds(d / t);
                }
            }

            public static unsafe int Size => sizeof(MovieHeaderAtom);

            public static unsafe MovieHeaderAtom FromByteArray(byte[] arr) {
                fixed (byte* ptr = arr) {
                    var ptr2 = (MovieHeaderAtom*)ptr;
                    return *ptr2;
                }
            }
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("video/mp4", "audio/mp4"))
                return null;

            async Task<byte[]?> GetRangeAsync(long start, long length) =>
                await originalLocation.GetRangeAsync(start, start + length);

            async IAsyncEnumerable<(uint offset, AtomHeader atom)> EnumerateAtomsAsync(uint offset = 0) {
                uint i = offset;
                while (await GetRangeAsync(i, 8) is byte[] arr) {
                    var hh = AtomHeader.FromByteArray(arr);
                    yield return (i, hh);
                    i += hh.length;
                }
            }

            await foreach (var (offset1, h) in EnumerateAtomsAsync())
                if (h.type == "moov")
                    await foreach (var (offset2, hh) in EnumerateAtomsAsync(offset: offset1 + 8))
                        if (hh.type == "mvhd")
                            if (await GetRangeAsync(offset2, MovieHeaderAtom.Size) is byte[] data)
                                return MovieHeaderAtom.FromByteArray(data).Duration;

            return null;
        }
    }
}
