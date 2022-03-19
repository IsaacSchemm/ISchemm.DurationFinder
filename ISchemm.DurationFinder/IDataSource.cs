using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDataSource {
        Task<byte[]?> GetRangeAsync(long from, long to);
        long? ContentLength { get; }
    }

    public class StreamDataSource : IDataSource {
        private readonly Stream stream;

        public long? ContentLength {
            get {
                try {
                    return stream.Length;
                } catch (NotSupportedException) {
                    return null;
                }
            }
        }

        public StreamDataSource(Stream stream) {
            this.stream = stream;
        }

        public async Task<byte[]?> GetRangeAsync(long from, long to) {
            stream.Seek(from, SeekOrigin.Begin);
            byte[] arr = new byte[to - from];
            return await stream.ReadAsync(arr, 0, arr.Length) == arr.Length
                ? arr
                : null;
        }
    }

    public class RemoteDataSource : IDataSource {
        private readonly Uri Uri;
        private readonly HttpContent? HttpContent;

        public long? ContentLength => HttpContent?.Headers?.ContentLength;

        public RemoteDataSource(Uri uri, HttpContent? httpContent) {
            Uri = uri;
            HttpContent = httpContent;
        }

        public async Task<byte[]?> GetRangeAsync(long from, long to) =>
            await Requests.GetRangeAsync(Uri, from, to);
    }
}
