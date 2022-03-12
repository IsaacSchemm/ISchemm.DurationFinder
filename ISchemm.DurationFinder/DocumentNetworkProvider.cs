using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class DocumentNetworkProvider : INetworkProvider {
        private readonly IDocumentProvider _documentProvider;

        public DocumentNetworkProvider(IDocumentProvider documentProvider) {
            _documentProvider = documentProvider;
        }

        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage) {
            if (!responseMessage.ContainsContentType("text/html", "application/xhtml+xml"))
                return null;

            await responseMessage.Content.LoadIntoBufferAsync();

            var document = new HtmlDocument();
            string html = await responseMessage.Content.ReadAsStringAsync();
            document.LoadHtml(html);
            return await _documentProvider.GetDurationAsync(document);
        }
    }
}
