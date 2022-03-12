using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDurationProvider {
        Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent);
    }
}
