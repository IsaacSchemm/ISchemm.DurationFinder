using HtmlAgilityPack;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace ISchemm.DurationFinder {
    public class SchemaOrgDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("text/html", "application/xhtml+xml"))
                return null;

            var document = new HtmlDocument();
            string html = await dataSource.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("itemprop", null) == "isLiveBroadcast")
                    if (node.GetAttributeValue("content", null) is string str)
                        if (bool.TryParse(str, out bool val))
                            if (val)
                                return null;

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("itemprop", null) == "duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return XmlConvert.ToTimeSpan(str);

            return null;
        }
    }
}
