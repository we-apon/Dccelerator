using System.Collections.Generic;


namespace Dccelerator.DataAccess {
    /// <summary>
    /// An manager that can get select some concrete data and get it from <seealso cref="IDataAccessRepository"/>, using some <see cref="IDataCriterion"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity, contains values that be getted with current selector instance.</typeparam>
    /// <typeparam name="TResult">Type of values, that will be getted with current selector instance</typeparam>
    public interface IDataSelector<TEntity, TResult> : IDataFilter<TEntity, IEnumerable<TResult>> where TEntity : class, new() {

    }
}