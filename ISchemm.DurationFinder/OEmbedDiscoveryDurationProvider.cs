using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedDiscoveryDurationProvider : IDurationProvider {
        private readonly OEmbedJsonDurationProvider _jsonProvider = new OEmbedJsonDurationProvider();

        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage) {
            if (responseMessage.Content.Headers.ContentType.MediaType != "text/html")
                if (responseMessage.Content.Headers.ContentType.MediaType != "application/xhtml+xml")
                    return null;

            var document = new HtmlDocument();
            string html = await responseMessage.Content.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("link"))
                if (node.GetAttributeValue("rel", null) == "alternate")
                    if (node.GetAttributeValue("type", null) == "application/json+oembed")
                        if (node.GetAttributeValue("href", null) is string str)
                            if (Uri.TryCreate(responseMessage.Headers.Location, HtmlEntity.DeEntitize(str), out var uri))
                                return await _jsonProvider.GetDurationAsync(uri);
            return null;
        }
    }
}
