using Dccelerator.DataAccess.Implementations.ReadingRepositories;


namespace Dccelerator.DataAccess.Implementations.DataGetters {

    /// <summary>
    /// Manager, that can get something from <seealso cref="IDataAccessRepository"/>, using some <see cref="IDataCriterion"/>.
    /// This manager is never caches anything.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    sealed class NotCachedDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {

        
        public NotCachedDataGetter() : base(new DirectReadingRepository()) {

        }
    }
}