using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class JsonDurationProvider : IDurationProvider {
        private class OEmbedResponse {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matching source schema")]
            public double? duration { get; set; }
        }

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("text/json", "application/json"))
                return null;

            byte[] data = await dataSource.ReadAsync();
            var obj = JsonSerializer.Deserialize<OEmbedResponse>(data);
            if (obj?.duration is double x)
                return TimeSpan.FromSeconds(x);

            return null;
        }
    }
}
