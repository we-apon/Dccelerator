using System;
using System.Collections.Concurrent;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess {
    /// <summary>
    /// Manager that combines all available data access capabilities
    /// </summary>
    public class DataManager : IDataManager {
        readonly IDataManagerFactory _factory;

        readonly ConcurrentDictionary<Type, object> _getters = new ConcurrentDictionary<Type, object>();
        readonly ConcurrentDictionary<Type, object> _countCheckers = new ConcurrentDictionary<Type, object>();
        readonly ConcurrentDictionary<Type, object> _existenceCheckers = new ConcurrentDictionary<Type, object>();


        /// <param name="factory">Factory used to instantinate data access tolls</param>
        public DataManager(IDataManagerFactory factory) {
            _factory = factory;
        }


        /// <summary>
        /// Returns an <see cref="IDataGetter{TEntity}"/> for getting that entity in cached context.
        /// </summary>
        public IDataGetter<TEntity> Get<TEntity>() where TEntity : class, new() {
            var type = RUtils<TEntity>.Type;

            object getter;
            if (_getters.TryGetValue(type, out getter))
                return (IDataGetter<TEntity>)getter;

            var dataGetter = _factory.GetterFor<TEntity>();
            if (!_getters.TryAdd(type, dataGetter))
                return (IDataGetter<TEntity>) _getters[type];

            return dataGetter;
        }


        /// <summary>
        /// Returns an <see cref="IDataGetter{TEntity}"/> for getting that entity in not cached context.
        /// Once entity will be getted - it also will be cached for global and current cache levels (or cache levels will be updated, if they already contains requested entities).
        /// </summary>
        public IDataGetter<TEntity> GetNotCached<TEntity>() where TEntity : class, new() {
            return _factory.NotCachedGetterFor<TEntity>();
        }


        /// <summary>
        /// Returns an <see cref="IDataExistenceChecker{TEntity}"/> for checking is some <typeparamref name="TEntity"/> exists in database.
        /// </summary>
        public IDataExistenceChecker<TEntity> Any<TEntity>() where TEntity : class
        {
            var type = RUtils<TEntity>.Type;

            object checker;
            if (_existenceCheckers.TryGetValue(type, out checker))
                return (IDataExistenceChecker<TEntity>)checker;

            var existenceChecker = _factory.DataExistenceChecker<TEntity>();
            if (!_existenceCheckers.TryAdd(type, existenceChecker))
                return (IDataExistenceChecker<TEntity>)_getters[type];

            return existenceChecker;
        }


        /// <summary>
        /// Returns an <see cref="IDataExistenceChecker{TEntity}"/> for checking is some <typeparamref name="TEntity"/> exists in database.
        /// Returned checked isn't used any cache.
        /// </summary>
        public IDataExistenceChecker<TEntity> AnyNotCached<TEntity>() where TEntity : class {
            return _factory.NoCachedExistenceChecker<TEntity>();
        }


        /// <summary>
        /// Returns an <see cref="IDataCountChecker{TEntity}"/> for getting count of some <typeparamref name="TEntity"/> in database.
        /// </summary>
        public IDataCountChecker<TEntity> CountOf<TEntity>() where TEntity : class {
            var type = RUtils<TEntity>.Type;

            object checker;
            if (_countCheckers.TryGetValue(type, out checker))
                return (IDataCountChecker<TEntity>)checker;

            var countChecker = _factory.DataCountChecker<TEntity>();
            if (!_countCheckers.TryAdd(type, countChecker))
                return (IDataCountChecker<TEntity>)_getters[type];

            return countChecker;
        }


        /// <summary>
        /// Returns an <see cref="IDataCountChecker{TEntity}"/> for getting count of some <typeparamref name="TEntity"/> in database.
        /// Returned checked isn't used any cache.
        /// </summary>
        public IDataCountChecker<TEntity> CountOfNotCached<TEntity>() where TEntity : class {
            return _factory.NoCachedDataCountChecker<TEntity>();
        }


        /// <summary>
        /// Returns new <see cref="IDataTransaction"/> that can be used for Inserting, Updating and Deleting entities 
        /// imnidiatelly or at some other good time.
        /// </summary>
        public IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            return _factory.DataTransaction(Scheduler, isolationLevel);
        }


        /// <summary>
        /// Returns <see cref="ITransactionScheduler"/> associated with current data manager.
        /// </summary>
        public ITransactionScheduler Scheduler {
            get {
                if (_scheduler != null)
                    return _scheduler;

                lock (_lock) {
                    if (_scheduler != null)
                        return _scheduler;

                    _scheduler = _factory.Scheduler();
                }

                return _scheduler;
            }
        }

        private volatile ITransactionScheduler _scheduler;
        private readonly object _lock = new object();
    }
}