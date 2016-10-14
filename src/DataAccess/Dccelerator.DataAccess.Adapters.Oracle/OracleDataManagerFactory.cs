using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public abstract class OracleDataManagerFactory<TRepository> :
            DataManagerAdoFactoryBase<TRepository, OracleEntityInfo<TRepository>>
        where TRepository : class, IAdoNetRepository {

        protected override DirectReadingRepository NotCachedReadingRepository<TEntity>() {
            return new OracleDirectReadingRepository();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public override IDataTransaction DataTransaction(ITransactionScheduler scheduler,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            return new OracleSimpleScheduledTransaction(scheduler, this, isolationLevel);
        }


        public override IReadingRepository ReadingRepository() {
            return new OracleCommonReadingRepository();
        }

    }
}