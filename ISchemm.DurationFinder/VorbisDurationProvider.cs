﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class VorbisDurationProvider : IDurationProvider {
        private readonly bool _searchEntireFile;

        public VorbisDurationProvider(bool searchEntireFile = false) {
            _searchEntireFile = searchEntireFile;
        }

        private class OggPageHeader {
            public readonly IDataSource DataSource;
            public readonly uint Start;

            public readonly ulong GranulePosition;
            public readonly byte[] SegmentTable;

            public uint SegmentTableStart => Start + 27;
            public uint SegmentTableEnd => SegmentTableStart + checked((uint)SegmentTable.LongLength);

            public uint End {
                get {
                    long i = SegmentTableEnd;
                    foreach (byte b in SegmentTable)
                        i += b;
                    return checked((uint)i);
                }
            }

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
                uint offset = SegmentTableEnd;
                foreach (byte segmentLength in SegmentTable) {
                    byte[]? segment = await DataSource.GetRangeAsync(offset, offset + segmentLength);
                    if (segment == null)
                        yield break;

                    yield return segment;
                    offset += segmentLength;
                }
            }

            public async Task<byte[]> ReadAsync() {
                using var ms = new MemoryStream();
                await foreach (byte[] segment in GetSegmentsAsync())
                    await ms.WriteAsync(segment, 0, segment.Length);
                return ms.ToArray();
            }

            public async Task<OggPageHeader?> GetNextAsync() =>
                await GetAsync(DataSource, End);
        }

        private async IAsyncEnumerable<OggPageHeader> EnumerateOggPageHeadersAsync(IDataSource dataSource) {
            var header = await OggPageHeader.GetAsync(dataSource);
            while (header != null) {
                yield return header;
                header = await header.GetNextAsync();
            }
        }

        private static async Task<ulong?> GetLastGranulePositionAsync(IDataSource dataSource) {
            if (dataSource.ContentLength is long contentLength) {
                byte[]? data = await dataSource.GetRangeAsync(
                    Math.Max(contentLength - 65307, 0),
                    contentLength - 1);
                if (data != null) {
                    for (int i = data.Length - 4; i >= 0; i--) {
                        if (data[i+0] != 'O') continue;
                        if (data[i+1] != 'g') continue;
                        if (data[i+2] != 'g') continue;
                        if (data[i+3] != 'S') continue;

                        var page = await OggPageHeader.GetAsync(new StreamDataSource(data), (uint)i);
                        if (page != null) {
                            return page.GranulePosition;
                        }
                    }
                }
            }
            return null;
        }

        private static TimeSpan? GetDuration(double? granulePosition, double? sampleRate) {
            if (sampleRate is double s && granulePosition is double g)
                return TimeSpan.FromSeconds(g / s);
            else
                return null;
        }

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("video/ogg", "audio/ogg"))
                return null;

            ulong maxGranulePosition = 0;

            uint? sampleRate = null;

            await foreach (var pageHeader in EnumerateOggPageHeadersAsync(dataSource)) {
                if (pageHeader.End - pageHeader.SegmentTableEnd == 0x1E) {
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

                if (sampleRate != null && _searchEntireFile == false)
                    return GetDuration(await GetLastGranulePositionAsync(dataSource), sampleRate);

                maxGranulePosition = Math.Max(maxGranulePosition, pageHeader.GranulePosition);
            }

            return GetDuration(maxGranulePosition, sampleRate);
        }
    }
}
