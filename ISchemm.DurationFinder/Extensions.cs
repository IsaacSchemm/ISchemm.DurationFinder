using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public static class Extensions {
        public const string UserAgentString = "ISchemm.DurationFinder/3.0 (https://github.com/IsaacSchemm/ISchemm.DurationFinder)";

        public static async Task<TimeSpan?> GetDurationAsync(this INetworkProvider provider, Uri uri) {
            var req = WebRequest.CreateHttp(uri);
            req.UserAgent = UserAgentString;
            req.Method = "HEAD";

            var resp = await req.GetResponseAsync();
            return await provider.GetDurationAsync(uri, new ContentType(resp.ContentType ?? "application/octet-stream"));
        }
    }
}
