using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedDurationProvider : IDurationProvider {
        private readonly IDurationProvider _jsonProvider = new JsonDurationProvider();

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("text/html", "application/xhtml+xml"))
                return null;

            var document = new HtmlDocument();
            string html = await httpContent.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("link"))
                if (node.GetAttributeValue("rel", null) == "alternate")
                    if (node.GetAttributeValue("type", null) == "application/json+oembed")
                        if (node.GetAttributeValue("href", null) is string str)
                            if (Uri.TryCreate(originalLocation, HtmlEntity.DeEntitize(str), out var uri))
                                return await _jsonProvider.GetDurationAsync(uri);

            return null;
        }
    }
}
