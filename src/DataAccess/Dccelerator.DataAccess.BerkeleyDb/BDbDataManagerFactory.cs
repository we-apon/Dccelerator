using System;
using Dccelerator.DataAccess.BerkeleyDb.Implementation;
using Dccelerator.DataAccess.BerkeleyDb.Infrastructure;
using Dccelerator.DataAccess.Implementation;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbDataManagerFactory : IDataManagerBDbFactory, IDisposable {
        readonly IBDbSchema _schema;


        public BDbDataManagerFactory(string environmentPath, string dbFilePath, string password) {
            _schema = new BDbSchema(environmentPath, dbFilePath, password);
        }

        public BDbDataManagerFactory(string environmentPath, string dbFilePath) : this(environmentPath, dbFilePath, null) { }


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
            return new BDbDataGetter<TEntity>(ReadingRepository(), BerkeleyInfoAbout<TEntity>());
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        public IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called on each delete request.
        /// </summary>
        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        public IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel) {
            return new NotScheduledBDbTransaction(this);
        }


        /// <summary>
        /// Instantinate an <see cref="ITransactionScheduler"/>.
        /// This method will be called one time in every <see cref="IDataManager"/>.
        /// </summary>
        public ITransactionScheduler Scheduler() {
            return new DummyScheduler(); //todo: test it!
        }


        /// <summary>
        /// Returns information about <typeparamref name="TEntity"/>.
        /// </summary>
        /// <seealso cref="IDataManagerFactory.InfoAbout"/>
        public IEntityInfo InfoAbout<TEntity>() {
            return BerkeleyInfoAbout<TEntity>();
        }


        /// <summary>
        /// Returns information about <paramref name="entityType"/>.
        /// </summary>
        /// <seealso cref="IDataManagerFactory.InfoAbout{TEntity}"/>
        public IEntityInfo InfoAbout(Type entityType) {
            var info = new BdbInfoAboutEntity(entityType).Info;
            if (info.Repository == null)
                info.Repository = Repository();

            return info;
        }


        public IReadingRepository ReadingRepository() {
            return BDbReadingRepository.Instance;
        }
        

        /// <summary>
        /// Returns information about <typeparamref name="TEntity"/>.
        /// </summary>
        public IBDbEntityInfo BerkeleyInfoAbout<TEntity>() {
            var info = BDbInfoAbout<TEntity>.Info;
            if (info.Repository == null)
                info.Repository = Repository();

            return info;
        }


        public virtual IBDbRepository Repository() {
            return new BDbRepository(Schema());
        }


        public IBDbSchema Schema() {
            return _schema;
        }

        #endregion


        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            _schema.Dispose();
        }

        #endregion
    }
}