using Dccelerator.DataAccess.Implementation;

namespace Dccelerator.DataAccess.Ado.Implementation.Internal {

    /// <summary>
    /// Manager, that can get something from <seealso cref="IAdoNetRepository"/>, using some <see cref="IDataCriterion"/>.
    /// This manager always caches everything.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    sealed class ForcedCacheDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        public ForcedCacheDataGetter(IReadingRepository readingRepository, IEntityInfo info) : base(readingRepository, info) { }

    }
}