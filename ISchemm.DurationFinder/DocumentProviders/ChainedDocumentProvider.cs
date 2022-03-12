using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder.DocumentProviders {
    public class ChainedDocumentProvider : IDocumentProvider {
        private readonly IReadOnlyList<IDocumentProvider> _providers;

        public ChainedDocumentProvider(IEnumerable<IDocumentProvider> providers) {
            _providers = providers.ToArray();
        }

        public ChainedDocumentProvider(params IDocumentProvider[] providers) {
            _providers = providers.ToArray();
        }

        public async Task<TimeSpan?> GetDurationAsync(HtmlDocument document) {
            foreach (var provider in _providers)
                if (await provider.GetDurationAsync(document) is TimeSpan ts)
                    return ts;
            return null;
        }
    }
}
