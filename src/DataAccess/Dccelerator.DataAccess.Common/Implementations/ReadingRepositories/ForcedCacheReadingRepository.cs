using System;


namespace Dccelerator.DataAccess.Implementations.ReadingRepositories {
    internal sealed class ForcedCacheReadingRepository : CachedReadingRepository
    {
        #region Overrides of CachedReadingRepository

        protected override TimeSpan CacheTimeoutOf(Type entityType) {
            return TimeSpan.MaxValue;
        }

        #endregion
    }
}