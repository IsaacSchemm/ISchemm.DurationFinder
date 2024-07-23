using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class HlsDurationProvider : IDurationProvider {
        private readonly IDurationProvider _chunklistProvider = new ChunklistDurationProvider();

        public static IEnumerable<string> KnownMediaTypes {
            get {
                foreach (string main in new[] { "application", "audio" })
                    foreach (string prefix in new[] { "x-", "" })
                        foreach (string sub in new[] { "mpegurl", "vnd.apple.mpegurl" })
                            yield return $"{main}/{prefix}{sub}";
            }
        }

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType(KnownMediaTypes.ToArray())) return null;

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
