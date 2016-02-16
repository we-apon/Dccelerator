using System;

namespace Dccelerator.DataAccess.Ado.Implementation {
    public abstract class ForcedCacheReadingRepository : CachedReadingRepository
    {
        #region Overrides of CachedReadingRepository

        protected override TimeSpan CacheTimeoutOf(IAdoEntityInfo info) {
            return TimeSpan.MaxValue;
        }

        #endregion
    }
}