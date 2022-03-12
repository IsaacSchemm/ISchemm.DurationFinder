using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder.NetworkProviders {
    public class DocumentNetworkProvider : INetworkProvider {
        private readonly IDocumentProvider _documentProvider;

        public DocumentNetworkProvider(IDocumentProvider documentProvider) {
            _documentProvider = documentProvider;
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri uri, ContentType contentType) {
            switch (contentType.MediaType) {
                case "text/html":
                case "application/xhtml+xml":
                    break;
                default:
                    return null;
            }

            var web = new HtmlWeb();
            var document = await web.LoadFromWebAsync(uri, encoding: null, credentials: null);
            return await _documentProvider.GetDurationAsync(document);
        }
    }
}
