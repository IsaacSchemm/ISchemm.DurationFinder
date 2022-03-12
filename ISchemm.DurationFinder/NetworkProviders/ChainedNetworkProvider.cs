using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder.NetworkProviders {
    public class ChainedNetworkProvider : INetworkProvider {
        private readonly IReadOnlyList<INetworkProvider> _providers;

        public ChainedNetworkProvider(IEnumerable<INetworkProvider> providers) {
            _providers = providers.ToArray();
        }

        public ChainedNetworkProvider(params INetworkProvider[] providers) {
            _providers = providers.ToArray();
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri uri, ContentType contentType) {
            foreach (var provider in _providers)
                if (await provider.GetDurationAsync(uri, contentType) is TimeSpan ts)
                    return ts;
            return null;
        }
    }
}
