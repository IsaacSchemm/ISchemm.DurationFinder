using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDurationProvider {
        Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage, IEnumerable<IDurationProvider> linkHandlers);
    }
}
