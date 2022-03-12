using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OpenGraphDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage, IEnumerable<IDurationProvider> linkHandlers) {
            if (responseMessage.Content.Headers.ContentType.MediaType != "text/html")
                if (responseMessage.Content.Headers.ContentType.MediaType != "application/xhtml+xml")
                    return null;

            var document = new HtmlDocument();
            string html = await responseMessage.Content.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("property", null) == "video:duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return TimeSpan.FromSeconds(double.Parse(str));
            return null;
        }
    }
}
