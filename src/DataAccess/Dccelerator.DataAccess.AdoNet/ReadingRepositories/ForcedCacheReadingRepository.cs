using System;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado.ReadingRepositories {
    public abstract class ForcedCacheReadingRepository : CachedReadingRepository
    {
        #region Overrides of CachedReadingRepository

        protected override TimeSpan CacheTimeoutOf(IAdoEntityInfo info) {
            return TimeSpan.MaxValue;
        }

        #endregion
    }
}