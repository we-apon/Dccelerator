using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    public abstract class SqlClientDataManagerFactory<TRepository> : DataManagerAdoFactoryBase<SqlEntityInfo<TRepository>> where TRepository : class, IAdoNetRepository {

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


        public override IReadingRepository ReadingRepository() {
            return new SqlClientForcedCacheReadingRepository();
        }

    }
}