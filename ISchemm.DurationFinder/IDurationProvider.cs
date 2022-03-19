using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDurationProvider {
        Task<TimeSpan?> GetDurationAsync(IDataSource dataSource);
    }
}
