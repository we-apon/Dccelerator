using System;


namespace Dccelerator.DataAccess.Lazy
{
    public class DataManagerFactoryLazyDecorator : IDataManagerFactory {
        readonly IDataManagerFactory _factory;


        public DataManagerFactoryLazyDecorator(IDataManagerFactory factory) {
            _factory = factory;
        }


        #region Implementation of IDataManagerFactory

        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in cached context.
        /// This method will be called one time for each <typeparamref name="TEntity"/> requested in each data manager.
        /// </summary>
        public IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new() {
            return NotCachedGetterFor<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in not cached context.
        /// This method will be called on each request of any not cached entity.
        /// </summary>
        public IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new() {
            return new LazyDataGetter<TEntity>(_factory.GetterFor<TEntity>(), this);
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        public IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            return _factory.DataExistenceChecker<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called on each delete request.
        /// </summary>
        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            return _factory.NoCachedExistenceChecker<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        public IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            return _factory.DataCountChecker<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            return _factory.NoCachedDataCountChecker<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel) {
            return _factory.DataTransaction(scheduler, isolationLevel);
        }


        /// <summary>
        /// Instantinate an <see cref="ITransactionScheduler"/>.
        /// This method will be called one time in every <see cref="IDataManager"/>.
        /// </summary>
        public ITransactionScheduler Scheduler() {
            return _factory.Scheduler();
        }


        /// <summary>
        /// Returns information about <typeparamref name="TEntity"/>.
        /// </summary>
        public IEntityInfo InfoAbout<TEntity>() {
            return _factory.InfoAbout<TEntity>();
        }


        /// <summary>
        /// Returns information about <paramref name="entityType"/>.
        /// </summary>
        /// <seealso cref="IDataManagerFactory.InfoAbout{TEntity}"/>
        public IEntityInfo InfoAbout(Type entityType) {
            return _factory.InfoAbout(entityType);
        }


        public IInternalReadingRepository ReadingRepository() {
            return _factory.ReadingRepository();
        }

        #endregion
    }

}
