namespace Dccelerator.DataAccess.BasicImplementation {
    /// <summary>
    /// Manager, that can get something from <seealso cref="IDataAccessRepository"/>, using some <see cref="IDataCriterion"/>.
    /// This manager uses cache only if that was configured for <typeparamref name="TEntity"/>, using <see cref="GloballyCachedEntityAttribute"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    sealed class ConfigurationRespectfullDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        public ConfigurationRespectfullDataGetter(IReadingRepository readingRepository, IEntityInfo info) : base(readingRepository, info) {}

    }
}