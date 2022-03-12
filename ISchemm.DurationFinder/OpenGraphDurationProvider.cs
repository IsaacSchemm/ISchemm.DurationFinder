using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OpenGraphDurationProvider : IDurationProvider {
        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (httpContent.Headers.ContentType.MediaType != "text/html")
                if (httpContent.Headers.ContentType.MediaType != "application/xhtml+xml")
                    return null;

            var document = new HtmlDocument();
            string html = await httpContent.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("meta"))
                if (node.GetAttributeValue("property", null) == "video:duration")
                    if (node.GetAttributeValue("content", null) is string str)
                        return TimeSpan.FromSeconds(double.Parse(str));
            return null;
        }
    }
}
