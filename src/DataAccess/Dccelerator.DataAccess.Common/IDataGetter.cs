using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace Dccelerator.DataAccess {
    /// <summary>
    /// An manager that can get something from <seealso cref="IDataAccessRepository"/>, using some <see cref="IDataCriterion"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    public interface IDataGetter<TEntity> : IDataFilter<TEntity, IEnumerable<TEntity>> where TEntity : class, new() {

        /// <summary>
        /// Selects value <paramref name="member"/> of <typeparamref name="TEntity"/>, getted with followed filtration.
        /// </summary>
        /// <returns><see cref="IDataSelector{TEntity, TValue}"/>. Use some of 'Where' methods of <see cref="IDataSelector{TEntity,TResult}"/> to specify criteria</returns>
        IDataSelector<TEntity, TValue> Select<TValue>(Expression<Func<TEntity, TValue>> member);

    }
}