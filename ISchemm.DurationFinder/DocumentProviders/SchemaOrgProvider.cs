using HtmlAgilityPack;
using System;
using System.Xml;

namespace ISchemm.DurationFinder.DocumentProviders {
    internal class SchemaOrgProvider : IDocumentProvider {
        public TimeSpan? GetDuration(HtmlDocument document) {
            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("itemprop", null) == "duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return XmlConvert.ToTimeSpan(str);
            return null;
        }
    }
}
