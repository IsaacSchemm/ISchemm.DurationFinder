using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class MP4DurationProvider : IDurationProvider {
        private class AtomHeader {
            public readonly IDataSource DataSource;
            public readonly uint Start;
            public readonly uint Length;
            public readonly uint Type;

            public uint End => Start + Length;

            private AtomHeader(IDataSource dataSource, uint start, uint length, uint type) {
                DataSource = dataSource;
                Start = start;
                Length = length;
                Type = type;
            }

            public static async Task<AtomHeader?> GetAsync(IDataSource dataSource, uint offset = 0) {
                return await dataSource.GetRangeAsync(offset, offset + 8) is byte[] arr
                    ? new AtomHeader(
                        dataSource: dataSource,
                        start: offset,
                        length: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(0, 4)),
                        type: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(4, 4)))
                    : null;
            }

            public async Task<AtomHeader?> GetFirstChildAsync() =>
                await GetAsync(DataSource, Start + 8);

            public async Task<AtomHeader?> GetNextAsync() =>
                await GetAsync(DataSource, End);

            public async Task<byte[]?> ReadAsync() =>
                await DataSource.GetRangeAsync(Start, End);
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
                return await header.DataSource.GetRangeAsync(header.Start, header.End) is byte[] arr && arr.Length == 108
                    ? new MovieHeaderAtom(
                        timeScale: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(20, 4)),
                        duration: BinaryPrimitives.ReadUInt32BigEndian(arr.AsSpan(24, 4)))
                    : null;
            }
        }

        private static readonly uint _moov = BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes("moov").AsSpan(0, 4));
        private static readonly uint _mvhd = BinaryPrimitives.ReadUInt32BigEndian(Encoding.ASCII.GetBytes("mvhd").AsSpan(0, 4));

        private async IAsyncEnumerable<AtomHeader> EnumerateAtomsAsync(IDataSource dataSource, AtomHeader? parent = null) {
            var atom = parent is AtomHeader a
                ? await a.GetFirstChildAsync()
                : await AtomHeader.GetAsync(dataSource);
            while (atom != null) {
                yield return atom;
                atom = await atom.GetNextAsync();
            }
        }

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("video/mp4", "audio/mp4"))
                return null;

            await foreach (var atom1 in EnumerateAtomsAsync(dataSource))
                if (atom1.Type == _moov)
                    await foreach (var atom2 in EnumerateAtomsAsync(dataSource, atom1))
                        if (atom2.Type == _mvhd && await MovieHeaderAtom.ReadAsync(atom2) is MovieHeaderAtom mvhd)
                            return mvhd.Duration;
            return null;
        }
    }
}
