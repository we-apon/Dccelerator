using System.Data;
using Dccelerator.DataAccess.Ado.DataGetters;
using Dccelerator.DataAccess.Ado.ReadingRepositories;
using Dccelerator.DataAccess.Implementations;
using Dccelerator.DataAccess.Implementations.DataExistenceChecker;
using Dccelerator.DataAccess.Implementations.Schedulers;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado {
    public abstract class DataManagerFactoryBase : IDataManagerFactory {

        /// <summary>
        /// Instantinate an <see cref="ForcedCacheDataGetter{TEntity}"/>, that will be used in cached context.
        /// This method will be called one time for each <typeparamref name="TEntity"/> requested in each data manager.
        /// </summary>
        public virtual IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new() {
            return new ForcedCacheDataGetter<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in not cached context.
        /// This method will be called on each request of any not cached entity.
        /// </summary>
        public virtual IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new() {
            return new NotCachedDataGetter<TEntity>();
        }
        


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public virtual IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            return new DataExistenceChecker<TEntity>(new ForcedCacheReadingRepository(), InfoAbout<TEntity>().EntityName);
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called on each delete request.
        /// </summary>
        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            return new DataExistenceChecker<TEntity>(new DirectReadingRepository(), InfoAbout<TEntity>().EntityName);
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public virtual IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            return new DataCountChecker<TEntity>(new ForcedCacheReadingRepository(), InfoAbout<TEntity>().EntityName);
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            return new DataCountChecker<TEntity>(new DirectReadingRepository(), InfoAbout<TEntity>().EntityName);
        }


/*
        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            return new SimpleScheduledTransaction(scheduler, this, isolationLevel);
//            return new NotScheduledDataTransaction(this, isolationLevel);
        }
*/


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public abstract IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);


        /// <summary>
        /// Instantinate an <see cref="ITransactionScheduler"/>.
        /// This method will be called one time in every <see cref="IDataManager"/>.
        /// </summary>
        public ITransactionScheduler Scheduler() {
            return new DummyScheduler();
        }


        public IEntityInfo InfoAbout<TEntity>() {
            throw new System.NotImplementedException();
        }
    }
}