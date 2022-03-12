﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class ChainedDurationProvider : IDurationProvider {
        private readonly IReadOnlyList<IDurationProvider> _providers;

        public ChainedDurationProvider(IEnumerable<IDurationProvider> providers) {
            _providers = providers.ToArray();
        }

        public ChainedDurationProvider(params IDurationProvider[] providers) {
            _providers = providers.ToArray();
        }

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            foreach (var provider in _providers)
                if (await provider.GetDurationAsync(originalLocation, httpContent) is TimeSpan ts)
                    return ts;
            return null;
        }
    }
}
