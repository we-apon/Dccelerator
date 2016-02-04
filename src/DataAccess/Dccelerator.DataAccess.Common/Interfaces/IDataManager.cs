


namespace Dccelerator.DataAccess {
    /// <summary>
    /// An manager that combines all available data access capabilities
    /// </summary>
    public interface IDataManager {

        /// <summary>
        /// Returns an <see cref="IDataGetter{TEntity}"/> for getting that entity in cached context.
        /// </summary>
        IDataGetter<TEntity> Get<TEntity>() where TEntity : class, new();


        /// <summary>
        /// Returns an <see cref="IDataGetter{TEntity}"/> for getting that entity in not cached context.
        /// Once entity will be getted - it also will be cached for global and current cache levels (or cache levels will be updated, if they already contains requested entities).
        /// </summary>
        IDataGetter<TEntity> GetNotCached<TEntity>() where TEntity : class, new();


        /// <summary>
        /// Returns an <see cref="IDataExistenceChecker{TEntity}"/> for checking is some <typeparamref name="TEntity"/> exists in database.
        /// </summary>
        IDataExistenceChecker<TEntity> Any<TEntity>() where TEntity : class;


        /// <summary>
        /// Returns an <see cref="IDataExistenceChecker{TEntity}"/> for checking is some <typeparamref name="TEntity"/> exists in database.
        /// Returned checked isn't used any cache.
        /// </summary>
        IDataExistenceChecker<TEntity> AnyNotCached<TEntity>() where TEntity : class;



        /// <summary>
        /// Returns an <see cref="IDataCountChecker{TEntity}"/> for getting count of some <typeparamref name="TEntity"/> in database.
        /// </summary>
        IDataCountChecker<TEntity> CountOf<TEntity>() where TEntity : class;


        /// <summary>
        /// Returns an <see cref="IDataCountChecker{TEntity}"/> for getting count of some <typeparamref name="TEntity"/> in database.
        /// Returned checked isn't used any cache.
        /// </summary>
        IDataCountChecker<TEntity> CountOfNotCached<TEntity>() where TEntity : class;


        /// <summary>
        /// Returns new <see cref="IDataTransaction"/> that can be used for Inserting, Updating and Deleting entities 
        /// imnidiatelly or at some other good time.
        /// </summary>
        IDataTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Returns <see cref="ITransactionScheduler"/> associated with current data manager.
        /// </summary>
        ITransactionScheduler Scheduler { get; }
    }
}