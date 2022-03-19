using System;
using System.IO;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class ChunklistDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("application/x-mpegURL", "application/x-vnd.apple.mpegURL", "audio/mpegURL", "audio/x-mpegURL"))
                return null;

            byte[] body = await dataSource.ReadAsync();
            using var sr = new StreamReader(new MemoryStream(body));

            TimeSpan ts = TimeSpan.Zero;

            string line = "";
            while ((line = await sr.ReadLineAsync()) != null)
                if (line.StartsWith("#EXTINF:"))
                    ts += TimeSpan.FromSeconds(double.Parse(line.Substring("#EXTINF:".Length).Split(',')[0]));
                else if (line == "#EXT-X-ENDLIST")
                    return ts;

            return null;
        }
    }
}
