using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedDiscoveryDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage, IEnumerable<IDurationProvider> linkHandlers) {
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
                            if (Uri.TryCreate(str, UriKind.Absolute, out var uri))
                                foreach (var linkHandler in linkHandlers)
                                    if (await linkHandler.GetDurationAsync(uri) is TimeSpan ts)
                                        return ts;
            return null;
        }
    }
}
