using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OpenGraphDocumentProvider : IDocumentProvider {
        public TimeSpan? GetDuration(HtmlDocument document) {
            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("property", null) == "video:duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return TimeSpan.FromSeconds(double.Parse(str));
            return null;
        }

        public Task<TimeSpan?> GetDurationAsync(HtmlDocument document) =>
            Task.FromResult(GetDuration(document));
    }
}
