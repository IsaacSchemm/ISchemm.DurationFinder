using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class ChainedNetworkProvider : INetworkProvider {
        private readonly IReadOnlyList<INetworkProvider> _providers;

        public ChainedNetworkProvider(IEnumerable<INetworkProvider> providers) {
            _providers = providers.ToArray();
        }

        public ChainedNetworkProvider(params INetworkProvider[] providers) {
            _providers = providers.ToArray();
        }

        public async Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage) {
            foreach (var provider in _providers)
                if (await provider.GetDurationAsync(responseMessage) is TimeSpan ts)
                    return ts;
            return null;
        }
    }
}
