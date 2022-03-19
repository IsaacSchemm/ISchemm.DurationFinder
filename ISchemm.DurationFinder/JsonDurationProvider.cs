using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class JsonDurationProvider : IDurationProvider {
        private class OEmbedResponse {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matching source schema")]
            public double? duration { get; set; }
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("text/json", "application/json"))
                return null;

            await httpContent.LoadIntoBufferAsync();

            var json = await httpContent.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<OEmbedResponse>(json);
            if (obj?.duration is double x)
                return TimeSpan.FromSeconds(x);

            return null;
        }
    }
}
