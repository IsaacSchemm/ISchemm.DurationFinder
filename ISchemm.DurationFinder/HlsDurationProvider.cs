using System;
using System.IO;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class HlsDurationProvider : IDurationProvider {
        private readonly IDurationProvider _chunklistProvider = new ChunklistDurationProvider();

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("application/x-mpegURL", "application/x-vnd.apple.mpegURL", "audio/mpegURL", "audio/x-mpegURL"))
                return null;

            byte[] body = await dataSource.ReadAsync();
            using var sr = new StreamReader(new MemoryStream(body));

            string line;
            while ((line = await sr.ReadLineAsync()) != null)
                if (!line.StartsWith("#") && line.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                    if (dataSource.TryCreateRelativeUri(line, out Uri chunklist))
                        if (await _chunklistProvider.GetDurationAsync(chunklist) is TimeSpan ts)
                            return ts;

            return null;
        }
    }
}
