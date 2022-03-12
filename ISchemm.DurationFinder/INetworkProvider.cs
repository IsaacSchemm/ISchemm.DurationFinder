using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface INetworkProvider {
        Task<TimeSpan?> GetDurationAsync(HttpResponseMessage responseMessage);
    }
}
