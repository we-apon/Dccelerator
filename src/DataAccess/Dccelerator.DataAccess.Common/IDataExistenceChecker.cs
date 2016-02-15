namespace Dccelerator.DataAccess {

    /// <summary>
    /// An manager, that checks is some <typeparamref name="TEntity"/> exists in database.
    /// </summary>
    public interface IDataExistenceChecker<TEntity> : IDataFilter<TEntity, bool> where TEntity : class { }
}