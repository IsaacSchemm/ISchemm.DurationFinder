using ISchemm.MP4Support;
using ISchemm.MP4Support.MetadataSources;
using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class MP4DurationProvider : IDurationProvider {
        private class MetadataSource : IMetadataSource {
            private readonly IDataSource _dataSource;

            public MetadataSource(IDataSource dataSource) {
                _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            }

            public Task<byte[]?> GetRangeAsync(long start, long end) {
                return _dataSource.GetRangeAsync(start, end);
            }

            void IDisposable.Dispose() { }
        }

        public async Task<TimeSpan?> GetDurationAsync(IDataSource dataSource) {
            if (!dataSource.MatchesType("video/mp4", "audio/mp4"))
                return null;

            var metadata = await MP4MetadataProvider.GetMetadataAsync(
                new MetadataSource(
                    dataSource));
            return metadata.Duration;
        }
    }
}
