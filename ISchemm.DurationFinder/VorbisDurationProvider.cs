using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class VorbisDurationProvider : IDurationProvider {
        private class OggPageHeader {
            public readonly IDataSource DataSource;
            public readonly uint Start;

            public readonly ulong GranulePosition;
            public readonly byte[] SegmentTable;

            public uint SegmentStart => Start + 27 + checked((uint)SegmentTable.Length);

            public uint SegmentLength {
                get {
                    long i = 0;
                    foreach (byte b in SegmentTable)
                        i += b;
                    return checked((uint)i);
                }
            }

            public uint SegmentEnd => SegmentStart + SegmentLength;

            public OggPageHeader(IDataSource dataSource, uint start, ulong granulePosition, byte[] segmentTable) {
                DataSource = dataSource;
                Start = start;
                GranulePosition = granulePosition;
                SegmentTable = segmentTable;
            }

            public static async Task<OggPageHeader?> GetAsync(IDataSource dataSource, uint offset = 0) {
                var arr = await dataSource.GetRangeAsync(offset, offset + 27 + 1);
                if (arr == null)
                    return null;
                if (arr[0] != 'O') return null;
                if (arr[1] != 'g') return null;
                if (arr[2] != 'g') return null;
                if (arr[3] != 'S') return null;
                byte segmentCount = arr[26];
                var segmentTable = segmentCount == 1
                    ? new[] { arr[27] }
                    : await dataSource.GetRangeAsync(offset + 27, offset + 27 + segmentCount);
                if (segmentTable == null)
                    return null;
                return new OggPageHeader(
                    dataSource: dataSource,
                    start: offset,
                    granulePosition: BinaryPrimitives.ReadUInt64LittleEndian(arr.AsSpan(6, 8)),
                    segmentTable: segmentTable);
            }

            public async IAsyncEnumerable<byte[]> GetSegmentsAsync() {
                uint offset = SegmentStart;
                foreach (byte segmentLength in SegmentTable) {
                    byte[]? segment = await DataSource.GetRangeAsync(offset, offset + segmentLength);
                    if (segment == null)
                        yield break;

                    yield return segment;
                    offset += segmentLength;
                }
            }

            public async Task<byte[]> ReadAsync() {
                var segments = new List<byte[]>(SegmentTable.Length);
                await foreach (byte[] segment in GetSegmentsAsync())
                    segments.Add(segment);
                return segments.SelectMany(x => x).ToArray();
            }

            public async Task<OggPageHeader?> GetNextAsync() =>
                await GetAsync(DataSource, SegmentEnd);
        }

        private async IAsyncEnumerable<OggPageHeader> EnumerateOggPageHeadersAsync(IDataSource dataSource) {
            var header = await OggPageHeader.GetAsync(dataSource);
            while (header != null) {
                yield return header;
                header = await header.GetNextAsync();
            }
        }

        public async Task<TimeSpan?> GetDurationAsync(Stream stream) {
            return await GetDurationAsync(new StreamDataSource(stream));
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("video/ogg", "audio/ogg"))
                return null;

            return await GetDurationAsync(new RemoteDataSource(originalLocation));
        }

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            uint? sampleRate = null;
            double maxGranulePosition = 0;

            var granules = new List<ulong>();

            await foreach (var pageHeader in EnumerateOggPageHeadersAsync(dataSource)) {
                maxGranulePosition = Math.Max(maxGranulePosition, pageHeader.GranulePosition);
                granules.Add(pageHeader.GranulePosition);

                if (pageHeader.SegmentLength == 0x1E) {
                    byte[] segment = await pageHeader.ReadAsync();

                    int i = 0;
                    if (segment[i++] != 1) continue;
                    if (segment[i++] != 'v') continue;
                    if (segment[i++] != 'o') continue;
                    if (segment[i++] != 'r') continue;
                    if (segment[i++] != 'b') continue;
                    if (segment[i++] != 'i') continue;
                    if (segment[i++] != 's') continue;

                    sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(segment.AsSpan(12, 4));
                }
            }

            if (sampleRate is uint s)
                return TimeSpan.FromSeconds(maxGranulePosition / s);
            else
                return null;
        }
    }
}
