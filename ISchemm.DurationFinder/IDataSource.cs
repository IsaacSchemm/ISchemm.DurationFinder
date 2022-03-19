using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDataSource {
        long? ContentLength { get; }
        bool MatchesType(params string[] types);
        Task<byte[]> ReadAsync();
        Task<byte[]?> GetRangeAsync(long from, long to);
        bool TryCreateRelativeUri(string uriString, out Uri result);
    }

    public class StreamDataSource : IDataSource, IDisposable {
        private readonly Stream _stream;

        public long? ContentLength => _stream.Length;

        public StreamDataSource(Stream stream) {
            if (!stream.CanSeek)
                throw new Exception("Only seekable streams are supported");
            _stream = stream;
        }

        public StreamDataSource(byte[] data) : this(new MemoryStream(data)) { }

        public bool MatchesType(params string[] types) => true;

        public async Task<byte[]> ReadAsync() {
            _stream.Seek(0, SeekOrigin.Begin);
            using var ms = new MemoryStream();
            await _stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<byte[]?> GetRangeAsync(long from, long to) {
            _stream.Seek(from, SeekOrigin.Begin);
            byte[] arr = new byte[to - from];
            return await _stream.ReadAsync(arr, 0, arr.Length) == arr.Length
                ? arr
                : null;
        }

        public bool TryCreateRelativeUri(string uriString, out Uri result) =>
            Uri.TryCreate(uriString, UriKind.Absolute, out result);

        public void Dispose() => _stream.Dispose();
    }

    public class RemoteDataSource : IDataSource, IDisposable {
        private readonly HttpResponseMessage _responseMessage;

        private byte[]? _data;

        public long? ContentLength => _responseMessage?.Content?.Headers?.ContentLength;

        public RemoteDataSource(HttpResponseMessage responseMessage) {
            _responseMessage = responseMessage;
            _data = null;
        }

        public bool MatchesType(params string[] types) {
            foreach (string t in types)
                if (string.Equals(_responseMessage?.Content.Headers?.ContentType?.MediaType, t, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public async Task<byte[]> ReadAsync() {
            _data ??= await _responseMessage.Content.ReadAsByteArrayAsync();
            return _data;
        }

        public async Task<byte[]?> GetRangeAsync(long from, long to) {
            if (_data is byte[] cache) {
                using var ds = new StreamDataSource(cache);
                return await ds.GetRangeAsync(from, to);
            } else {
                using var req = new HttpRequestMessage(HttpMethod.Get, _responseMessage.RequestMessage.RequestUri);
                req.Headers.UserAgent.ParseAdd(Extensions.UserAgentString);
                req.Headers.Range = new RangeHeaderValue(from, to);

                using var resp = await Extensions.HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

                if (!(resp.Content.Headers.ContentRange is ContentRangeHeaderValue cr)) return null;
                if (!cr.HasRange) return null;
                if (cr.From != from) return null;
                if (cr.To < to) return null;
                if (cr.Unit != "bytes") return null;

                var stream = await resp.Content.ReadAsStreamAsync();
                var arr = new byte[to - from];
                int read = 0;
                while (true) {
                    int r = await stream.ReadAsync(arr, read, arr.Length - read);
                    if (r <= 0) break;
                    read += r;
                    if (read == arr.Length) break;
                }
                return read == arr.Length
                    ? arr
                    : null;
            }
        }

        public bool TryCreateRelativeUri(string uriString, out Uri result) =>
            Uri.TryCreate(_responseMessage.RequestMessage.RequestUri, uriString, out result);

        public void Dispose() => _responseMessage.Dispose();
    }
}
