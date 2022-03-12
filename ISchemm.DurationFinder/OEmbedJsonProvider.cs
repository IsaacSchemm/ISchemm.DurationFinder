using System;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedJsonProvider : INetworkProvider {
        private class OEmbedResponse {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matching source schema")]
            public double? duration { get; set; }
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri uri, ContentType contentType) {
            if (contentType.MediaType != "application/json")
                return null;

            var req = WebRequest.CreateHttp(uri);
            req.UserAgent = Extensions.UserAgentString;
            req.Method = "GET";

            using var resp = await req.GetResponseAsync();
            using var stream = resp.GetResponseStream();
            var obj = await JsonSerializer.DeserializeAsync<OEmbedResponse>(stream);
            if (obj?.duration is double x)
                return TimeSpan.FromSeconds(x);

            return null;
        }
    }
}
