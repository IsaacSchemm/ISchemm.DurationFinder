using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ISchemm.DurationFinder.DocumentProviders {
    public class ChainedDocumentProvider : IDocumentProvider {
        private readonly IReadOnlyList<IDocumentProvider> _providers;

        public ChainedDocumentProvider(IEnumerable<IDocumentProvider> providers) {
            _providers = providers.ToArray();
        }

        public ChainedDocumentProvider(params IDocumentProvider[] providers) {
            _providers = providers.ToArray();
        }

        public TimeSpan? GetDuration(HtmlDocument document) {
            foreach (var provider in _providers)
                if (provider.GetDuration(document) is TimeSpan ts)
                    return ts;
            return null;
        }
    }
}
