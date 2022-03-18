using System;
using System.IO;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDataSource {
        Task<byte[]?> GetRangeAsync(long from, long to);
    }

    public class StreamDataSource : IDataSource, IDisposable {
        private readonly Stream stream;

        public StreamDataSource(Stream stream) {
            this.stream = stream;
        }

        public void Dispose() => stream.Dispose();

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

        public RemoteDataSource(Uri uri) {
            Uri = uri;
        }

        public async Task<byte[]?> GetRangeAsync(long from, long to) => await Requests.GetRangeAsync(Uri, from, to);
    }
}
