using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class MP4DurationProvider : IDurationProvider {
        private class AtomHeader {
            public readonly Uri Uri;
            public readonly uint Start;
            public readonly uint Length;
            public readonly uint Type;

            public uint End => Start + Length;

            private AtomHeader(Uri uri, uint start, uint length, uint type) {
                Uri = uri;
                Start = start;
                Length = length;
                Type = type;
            }

            public static async Task<AtomHeader?> GetAsync(Uri uri, uint offset = 0) {
                return await uri.GetRangeAsync(offset, offset + 8) is byte[] arr
                    ? new AtomHeader(
                        uri: uri,
                        start: offset,
                        length: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(0, 4)),
                        type: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(4, 4)))
                    : null;
            }

            public async Task<AtomHeader?> GetFirstChildAsync() =>
                await GetAsync(Uri, Start + 8);

            public async Task<AtomHeader?> GetNextAsync() =>
                await GetAsync(Uri, End);

            public async Task<byte[]?> ReadAsync() =>
                await Uri.GetRangeAsync(Start, End);
        }

        private async IAsyncEnumerable<AtomHeader> EnumerateAtomsAsync(Uri originalLocation, AtomHeader? parent = null) {
            var atom = parent is AtomHeader a
                ? await a.GetFirstChildAsync()
                : await AtomHeader.GetAsync(originalLocation);
            while (atom != null) {
                yield return atom;
                atom = await atom.GetNextAsync();
            }
        }

        private class MovieHeaderAtom {
            private readonly double _timeScale;
            private readonly double _duration;

            public TimeSpan Duration => TimeSpan.FromSeconds(_duration / _timeScale);

            private MovieHeaderAtom(uint timeScale, uint duration) {
                _timeScale = timeScale;
                _duration = duration;
            }

            public static async Task<MovieHeaderAtom?> ReadAsync(AtomHeader header) {
                return await header.Uri.GetRangeAsync(header.Start, header.End) is byte[] arr && arr.Length == 108
                    ? new MovieHeaderAtom(
                        timeScale: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(20, 4)),
                        duration: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(24, 4)))
                    : null;
            }
        }

        private static readonly uint _moov = BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes("moov").AsSpan(0, 4));
        private static readonly uint _mvhd = BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes("mvhd").AsSpan(0, 4));

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("video/mp4", "audio/mp4"))
                return null;

            await foreach (var atom1 in EnumerateAtomsAsync(originalLocation))
                if (atom1.Type == _moov)
                    await foreach (var atom2 in EnumerateAtomsAsync(originalLocation, atom1))
                        if (atom2.Type == _mvhd && await MovieHeaderAtom.ReadAsync(atom2) is MovieHeaderAtom mvhd)
                            return mvhd.Duration;

            return null;
        }
    }
}
