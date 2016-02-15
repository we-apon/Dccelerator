using Dccelerator.DataAccess.Ado.ReadingRepositories;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    public abstract class SqlClientDataManagerFactory<TRepository> : DataManagerAdoFactoryBase<TRepository> where TRepository : class, IAdoNetRepository {
        #region Overrides of DataManagerAdoFactoryBase

        protected override ForcedCacheReadingRepository ForcedCachedReadingRepository<TEntity>() {
            return new SqlClientForcedCacheReadingRepository();
        }


        protected override DirectReadingRepository NotCachedReadingRepository<TEntity>() {
            return new SqlClientDirectReadingRepository();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public override IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            return new SqlClientSimpleScheduledTransaction(scheduler, this, isolationLevel);
        }


        public override IInternalReadingRepository ReadingRepository() {
            return new SqlClientCommonReadingRepository();
        }


        #endregion
    }
}