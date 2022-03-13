using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class MP4DurationProvider : IDurationProvider {
        private class AtomHeader {
            public int Start { get; set; }
            public int Length { get; set; }
            public string Name { get; set; } = "";
        }

        private class MemoryDataSource {
            private readonly Uri _uri;
            //private readonly byte[] _initialData;
            private readonly long _length;

            public MemoryDataSource(Uri uri, /*byte[] initialData,*/ long length) {
                _uri = uri;
                //_initialData = initialData;
                _length = length;
            }

            public async Task<byte[]?> GetRangeAsync(long start, long length) {
                //if (start + length < _initialData.Length)
                //    return _initialData.AsMemory().Slice(
                //        checked((int)start),
                //        checked((int)length)).ToArray();

                if (await _uri.GetRangeAsync(start, start + length) is byte[] arr)
                    return arr;

                return null;
            }

            public async IAsyncEnumerable<AtomHeader> EnumerateAtomsAsync(int offset = 0) {
                int i = offset;
                while (true) {
                    if (i + 8 > _length)
                        yield break;
                    var arr = await GetRangeAsync(i, 8);
                    if (arr == null)
                        yield break;
                    int length = arr[0] << 24 | arr[1] << 16 | arr[2] << 8 | arr[3];
                    string name = new string(arr.Skip(4).Select(x => (char)x).ToArray());
                    if (i + length > _length)
                        yield break;
                    yield return new AtomHeader {
                        Start = i,
                        Length = length,
                        Name = name
                    };
                    i += length;
                }
            }
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("video/mp4", "audio/mp4"))
                return null;

            //byte[] buffer = new byte[2048];
            //var stream = await httpContent.ReadAsStreamAsync();
            //int read = await stream.ReadAsync(buffer, 0, buffer.Length);

            var dataSource = new MemoryDataSource(originalLocation, /*buffer,*/ httpContent.Headers.ContentLength ?? /*read ??*/ 0);
            await foreach (var h in dataSource.EnumerateAtomsAsync()) {
                System.Diagnostics.Debug.WriteLine(h.Name);
                if (h.Name == "moov") {
                    await foreach (var hh in dataSource.EnumerateAtomsAsync(offset: h.Start + 8)) {
                        if (hh.Name == "mvhd") {
                            if (await dataSource.GetRangeAsync(hh.Start + 20, 8) is byte[] data) {
                                uint timeScale = 0;
                                for (int i = 0; i < 4; i++) {
                                    timeScale <<= 8;
                                    timeScale |= data[i];
                                }
                                uint duration = 0;
                                for (int i = 4; i < 8; i++) {
                                    duration <<= 8;
                                    duration |= data[i];
                                }
                                return TimeSpan.FromSeconds((double)duration / timeScale);
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
