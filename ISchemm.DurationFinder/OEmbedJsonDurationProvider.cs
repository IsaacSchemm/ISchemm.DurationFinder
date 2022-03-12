using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedJsonDurationProvider : IDurationProvider {
        private class OEmbedResponse {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matching source schema")]
            public double? duration { get; set; }
        }

        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage) {
            if (responseMessage.Content.Headers.ContentType.MediaType != "application/json")
                return null;

            await responseMessage.Content.LoadIntoBufferAsync();

            var json = await responseMessage.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<OEmbedResponse>(json);
            if (obj?.duration is double x)
                return TimeSpan.FromSeconds(x);

            return null;
        }
    }
}
