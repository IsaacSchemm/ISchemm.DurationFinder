using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OpenGraphDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("text/html", "application/xhtml+xml"))
                return null;

            var document = new HtmlDocument();
            string html = await dataSource.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("property", null) == "video:duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return TimeSpan.FromSeconds(double.Parse(str));

            return null;
        }
    }
}
