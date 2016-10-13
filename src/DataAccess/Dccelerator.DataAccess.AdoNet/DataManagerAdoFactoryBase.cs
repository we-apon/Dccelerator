using System;
using Dccelerator.DataAccess.Ado.Implementation;
using Dccelerator.DataAccess.Ado.Implementation.Internal;
using Dccelerator.DataAccess.Implementation;
using Dccelerator.DataAccess.Implementation.Internal;


namespace Dccelerator.DataAccess.Ado {
    public abstract class DataManagerAdoFactoryBase<TRepository, TEntityInfo> : IDataManagerAdoFactory 
        where TRepository : class, IAdoNetRepository
        where TEntityInfo : IAdoEntityInfo
    {

        protected abstract DirectReadingRepository NotCachedReadingRepository<TEntity>() where TEntity : class;

        public abstract IReadingRepository ReadingRepository();
        public abstract IAdoNetRepository AdoNetRepository();
        protected abstract TEntityInfo GetEntityInfo(Type type);


        /// <summary>
        /// Instantinate an <see cref="ForcedCacheDataGetter{TEntity}"/>, that will be used in cached context.
        /// This method will be called one time for each <typeparamref name="TEntity"/> requested in each data manager.
        /// </summary>
        public virtual IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new() {
            return new DefaultDataGetter<TEntity>(ReadingRepository(), InfoAbout<TEntity>());
        }


        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in not cached context.
        /// This method will be called on each request of any not cached entity.
        /// </summary>
        public virtual IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new() {
            return new DefaultDataGetter<TEntity>(NotCachedReadingRepository<TEntity>(), InfoAbout<TEntity>());
        }
        


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public virtual IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            return new DataExistenceChecker<TEntity>(ReadingRepository(), AdoInfoAbout<TEntity>());
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called on each delete request.
        /// </summary>
        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            return new DataExistenceChecker<TEntity>(NotCachedReadingRepository<TEntity>(), AdoInfoAbout<TEntity>());
        }




        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public virtual IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            return new DataCountChecker<TEntity>(ReadingRepository(), AdoInfoAbout<TEntity>());
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            return new DataCountChecker<TEntity>(NotCachedReadingRepository<TEntity>(), AdoInfoAbout<TEntity>());
        }


/*
        /// <summary>
        /// Instantiate an <see cref="IDataTransaction"/>.
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


        public virtual IEntityInfo InfoAbout<TEntity>() {
            return AdoInfoAbout<TEntity>();
        }


        /// <summary>
        /// Returns information about <paramref name="entityType"/>.
        /// </summary>
        /// <seealso cref="IDataManagerFactory.InfoAbout{TEntity}"/>
        public virtual IEntityInfo InfoAbout(Type entityType) {
            var info = new AdoNetInfoCacheContainer<TEntityInfo>(entityType, GetEntityInfo).Info;
            if (info.Repository == null) {
                lock (info) {
                    if (info.Repository == null)
                        info.SetupRepository(AdoNetRepository());
                }
            }
            return info;
        }



        

        public virtual IAdoEntityInfo AdoInfoAbout<TEntity>() {
            var info = AdoNetInfoCache<TEntityInfo, TEntity>.Info;
            if (info.Repository == null) {
                lock (info) {
                    if (info.Repository == null)
                        info.SetupRepository(AdoNetRepository());
                }
            }
            return info;
        }



    }

   
}