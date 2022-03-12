using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class HLSChunklistDurationProvider : IDurationProvider {
        private static bool IsHLS(string contentType) {
            var acceptable_types = new[] {
                "application/x-mpegURL",
                "application/x-vnd.apple.mpegURL",
                "audio/mpegURL"
            };

            foreach (string t in acceptable_types)
                if (string.Equals(contentType, t, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage) {
            if (!IsHLS(responseMessage.Content.Headers.ContentType.MediaType))
                return null;

            await responseMessage.Content.LoadIntoBufferAsync();

            using var stream = await responseMessage.Content.ReadAsStreamAsync();
            using var sr = new StreamReader(stream);

            TimeSpan ts = TimeSpan.Zero;

            string line = "";
            while ((line = await sr.ReadLineAsync()) != null) {
                if (line.StartsWith("#EXTINF:")) {
                    string[] split = line.Substring("#EXTINF:".Length).Split(',');
                    ts += TimeSpan.FromSeconds(double.Parse(split[0]));
                } else if (line == "#EXT-X-ENDLIST") {
                    return ts;
                }
            }

            return null;
        }
    }
}
