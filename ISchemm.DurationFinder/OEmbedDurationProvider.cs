using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedDurationProvider : IDurationProvider {
        public static readonly IDurationProvider JsonProvider = new OEmbedJsonDurationProvider();

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("text/html", "application/xhtml+xml"))
                return null;

            var document = new HtmlDocument();
            string html = await httpContent.ReadAsStringAsync();
            document.LoadHtml(html);

            foreach (var node in document.DocumentNode.Descendants("link"))
                if (node.GetAttributeValue("rel", null) == "alternate")
                    if (node.GetAttributeValue("type", null) == "application/json+oembed")
                        if (node.GetAttributeValue("href", null) is string str)
                            if (Uri.TryCreate(originalLocation, HtmlEntity.DeEntitize(str), out var uri))
                                return await JsonProvider.GetDurationAsync(uri);
            return null;
        }

        public class OEmbedJsonDurationProvider : IDurationProvider {
            private class OEmbedResponse {
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matching source schema")]
                public double? duration { get; set; }
            }

            public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
                if (!httpContent.IsOfType("text/json", "application/json"))
                    return null;

                await httpContent.LoadIntoBufferAsync();

                var json = await httpContent.ReadAsStringAsync();
                var obj = JsonSerializer.Deserialize<OEmbedResponse>(json);
                if (obj?.duration is double x)
                    return TimeSpan.FromSeconds(x);

                return null;
            }
        }
    }
}
