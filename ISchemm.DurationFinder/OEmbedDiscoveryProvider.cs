using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class OEmbedDiscoveryProvider : IDocumentProvider {
        private readonly OEmbedJsonProvider _oEmbedJsonProvider;

        public OEmbedDiscoveryProvider(OEmbedJsonProvider oEmbedJsonProvider) {
            _oEmbedJsonProvider = oEmbedJsonProvider;
        }

        public async Task<TimeSpan?> GetDurationAsync(HtmlDocument document) {
            foreach (var node in document.DocumentNode.Descendants("link"))
                if (node.GetAttributeValue("rel", null) == "alternate")
                    if (node.GetAttributeValue("type", null) == "application/json+oembed")
                        if (node.GetAttributeValue("href", null) is string str)
                            if (Uri.TryCreate(str, UriKind.Absolute, out var uri))
                                return await _oEmbedJsonProvider.GetDurationAsync(uri);
            return null;
        }
    }
}
