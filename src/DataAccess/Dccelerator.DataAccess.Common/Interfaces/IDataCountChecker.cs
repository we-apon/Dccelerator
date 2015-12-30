namespace Dccelerator.DataAccess {
    /// <summary>
    /// An manager, that finds count of some <typeparamref name="TEntity"/> in database.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IDataCountChecker<TEntity> : IDataFilter<TEntity, long> where TEntity : class { }
}