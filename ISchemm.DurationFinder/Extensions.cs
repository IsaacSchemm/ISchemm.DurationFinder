using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public static class Extensions {
        private static readonly string UserAgentString = "ISchemm.DurationFinder/3.0 (https://github.com/IsaacSchemm/ISchemm.DurationFinder)";

        private static readonly HttpClient _httpClient = new HttpClient();

        internal static bool ContainsContentType(this HttpResponseMessage response, params string[] contentTypes) =>
            contentTypes.Contains(response.Content.Headers.ContentType.MediaType);

        public static async Task<TimeSpan?> GetDurationAsync(this INetworkProvider provider, Uri uri) {
            using var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.UserAgent.ParseAdd(UserAgentString);
            using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            return await provider.GetDurationAsync(resp);
        }
    }
}
