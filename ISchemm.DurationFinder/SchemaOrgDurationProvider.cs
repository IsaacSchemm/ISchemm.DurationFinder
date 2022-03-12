using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ISchemm.DurationFinder {
    public class SchemaOrgDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage, IEnumerable<IDurationProvider> linkHandlers) {
            if (responseMessage.Content.Headers.ContentType.MediaType != "text/html")
                if (responseMessage.Content.Headers.ContentType.MediaType != "application/xhtml+xml")
                    return null;

            var document = new HtmlDocument();
            string html = await responseMessage.Content.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("itemprop", null) == "duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return XmlConvert.ToTimeSpan(str);
            return null;
        }
    }
}
