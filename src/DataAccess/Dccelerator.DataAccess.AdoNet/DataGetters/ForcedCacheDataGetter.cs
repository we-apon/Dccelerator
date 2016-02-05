using Dccelerator.DataAccess.Implementations.DataGetters;


namespace Dccelerator.DataAccess.Ado.DataGetters {

    /// <summary>
    /// Manager, that can get something from <seealso cref="IAdoNetRepository"/>, using some <see cref="IDataCriterion"/>.
    /// This manager always caches everything.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    sealed class ForcedCacheDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        public ForcedCacheDataGetter(IInternalReadingRepository readingRepository, IEntityInfo info) : base(readingRepository, info) { }

    }
}