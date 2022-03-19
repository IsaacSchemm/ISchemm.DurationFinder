using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedDurationProvider : IDurationProvider {
        private readonly IDurationProvider _jsonProvider = new JsonDurationProvider();

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("text/html", "application/xhtml+xml"))
                return null;

            var document = new HtmlDocument();
            string html = await dataSource.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("link"))
                if (node.GetAttributeValue("rel", null) == "alternate")
                    if (node.GetAttributeValue("type", null) == "application/json+oembed")
                        if (node.GetAttributeValue("href", null) is string str)
                            if (dataSource.TryCreateRelativeUri(HtmlEntity.DeEntitize(str), out Uri chunklist))
                                return await _jsonProvider.GetDurationAsync(chunklist);

            return null;
        }
    }
}
