using Dccelerator.DataAccess.Implementation;

namespace Dccelerator.DataAccess.Ado.Implementation.Internal {

    /// <summary>
    /// Manager, that can get something from <seealso cref="IAdoNetRepository"/>, using some <see cref="IDataCriterion"/>.
    /// This manager is never caches anything.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    sealed class NotCachedDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {

        
        public NotCachedDataGetter(IReadingRepository readingRepository, IEntityInfo info) : base(readingRepository, info) { }
    }
}