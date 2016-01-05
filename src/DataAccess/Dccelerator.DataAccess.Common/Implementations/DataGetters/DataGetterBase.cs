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
    abstract class DataGetterBase<TEntity> : DataFilterBase<TEntity, IEnumerable<TEntity>>, IDataGetter<TEntity> where TEntity : class, new() {
        protected internal readonly IInternalReadingRepository Repository;

        protected DataGetterBase(IInternalReadingRepository repository) {
            Repository = repository;
        }
        


        #region Implementations of IDataGetter<TEntity>

        /// <summary>
        /// Selects value <paramref name="member"/> of <typeparamref name="TEntity"/>, getted with followed filtration.
        /// </summary>
        /// <returns><see cref="IDataSelector{TEntity, TValue}"/>. Use some of 'Where' methods of <see cref="IDataSelector{TEntity,TResult}"/> to specify criteria</returns>
        public virtual IDataSelector<TEntity, TValue> Select<TValue>(Expression<Func<TEntity, TValue>> member) {
            return new DataSelector<TEntity, TValue>(Repository, member.Path());
        }

        #endregion


        #region Overrides of DataFilterBase<TEntity,IEnumerable<TEntity>>

        protected override IEnumerable<TEntity> ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return Repository.Read(Info.EntityName, EntityType, criteria).Cast<TEntity>();
        }

        #endregion
    }
}