

using System;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess {

    /// <summary>
    /// An factory used to instantinate implementations of data management tools.
    /// </summary>
    public interface IDataManagerFactory {

        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in cached context.
        /// This method will be called one time for each <typeparamref name="TEntity"/> requested in each data manager.
        /// </summary>
        IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new();


        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in not cached context.
        /// This method will be called on each request of any not cached entity.
        /// </summary>
        IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new();


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class;


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called on each delete request.
        /// </summary>
        IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class;


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class;

        
        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class;


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel);


        /// <summary>
        /// Instantinate an <see cref="ITransactionScheduler"/>.
        /// This method will be called one time in every <see cref="IDataManager"/>.
        /// </summary>
        ITransactionScheduler Scheduler();


        /// <summary>
        /// Returns information about <typeparamref name="TEntity"/>.
        /// </summary>
        /// <seealso cref="InfoAbout"/>
        IEntityInfo InfoAbout<TEntity>();


        /// <summary>
        /// Returns information about <paramref name="entityType"/>.
        /// </summary>
        /// <seealso cref="InfoAbout{TEntity}"/>
        IEntityInfo InfoAbout(Type entityType);


        IInternalReadingRepository ReadingRepository();
    }
}