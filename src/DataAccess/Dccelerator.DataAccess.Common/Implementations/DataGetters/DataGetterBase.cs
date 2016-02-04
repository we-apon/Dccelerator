using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations.DataGetters {

   
    /// <summary>
    /// Basic implementation of manager, that can get something from <seealso cref="IDataAccessRepository"/>, using some <see cref="IDataCriterion"/>.
    /// </summary>
    /// <typeparam name="TEntity">Entity that will be getted with current getter instance.</typeparam>
    public abstract class DataGetterBase<TEntity> : DataFilterBase<TEntity, IEnumerable<TEntity>>, IDataGetter<TEntity> where TEntity : class, new() {
        protected internal readonly IInternalReadingRepository Repository;
        readonly IEntityInfo _info;


        protected DataGetterBase(IInternalReadingRepository repository, IEntityInfo info) {
            Repository = repository;
            _info = info;
        }

        #region Implementations of IDataGetter<TEntity>

        /// <summary>
        /// Selects value <paramref name="member"/> of <typeparamref name="TEntity"/>, getted with followed filtration.
        /// </summary>
        /// <returns><see cref="IDataSelector{TEntity, TValue}"/>. Use some of 'Where' methods of <see cref="IDataSelector{TEntity,TResult}"/> to specify criteria</returns>
        public virtual IDataSelector<TEntity, TValue> Select<TValue>(Expression<Func<TEntity, TValue>> member) {
            return new DataSelector<TEntity, TValue>(Repository, _info, member.Path());
        }

        #endregion


        #region Overrides of DataFilterBase<TEntity,IEnumerable<TEntity>>

        protected override IEnumerable<TEntity> ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return Repository.Read(_info, criteria).Cast<TEntity>();
        }

        #endregion
    }
}