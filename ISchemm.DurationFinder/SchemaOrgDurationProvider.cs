using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ISchemm.DurationFinder {
    public class SchemaOrgDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("text/html", "application/xhtml+xml"))
                return null;

            var document = new HtmlDocument();
            string html = await httpContent.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("itemprop", null) == "duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return XmlConvert.ToTimeSpan(str);
            return null;
        }
    }
}
