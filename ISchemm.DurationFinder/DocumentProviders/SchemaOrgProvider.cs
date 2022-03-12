using HtmlAgilityPack;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace ISchemm.DurationFinder.DocumentProviders {
    public class SchemaOrgProvider : IDocumentProvider {
        public TimeSpan? GetDuration(HtmlDocument document) {
            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("itemprop", null) == "duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return XmlConvert.ToTimeSpan(str);
            return null;
        }

        public Task<TimeSpan?> GetDurationAsync(HtmlDocument document) =>
            Task.FromResult(GetDuration(document));
    }
}
