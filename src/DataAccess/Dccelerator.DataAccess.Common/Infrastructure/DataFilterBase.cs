using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace Dccelerator.DataAccess.Infrastructure {
    public abstract class DataFilterBase<TEntity, TResult> : CriterionGeneratorBase<TEntity>, IDataFilter<TEntity, TResult> where TEntity : class {

        // ReSharper disable once StaticMemberInGenericType
        static readonly IDataCriterion[] _emptyCriteria = new IDataCriterion[0];

        protected abstract TResult ApplyFilterWith(ICollection<IDataCriterion> criteria);


        #region Implementation of IDataFilter<TEntity,TResult>

        /// <summary>
        /// Returns all <typeparamref name="TEntity"/> what it can get
        /// </summary>
        public TResult All() {
            return ApplyFilterWith(_emptyCriteria);
        }




        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// Criteria will be getted from <paramref name="context"/>
        /// </summary>
        /// <param name="context">Context of <paramref name="expressions"/></param>
        /// <param name="expressions">Expression that selects filtering criteria</param>
        /// <returns><typeparamref name="TEntity"/> filtered with specified criteria.</returns>
        public TResult By<TContext>(TContext context, params Expression<Func<TContext, object>>[] expressions) {
            var criteria = GetCriteriaFrom(context, expressions);
            return ApplyFilterWith(criteria);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with <paramref name="criteria"/>.
        /// </summary>
        public TResult Where(params KeyValuePair<string, object>[] criteria) {
            var dataCriteria = GetCriteriaFrom(criteria);
            return ApplyFilterWith(dataCriteria);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with <paramref name="criteria"/>.
        /// </summary>
        public TResult Where(params KeyValuePair<Expression<Func<TEntity, object>>, object>[] criteria) {
            var dataCriteria = GetCriteriaFrom(criteria);
            return ApplyFilterWith(dataCriteria);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member">Expression that defines name of criterion</param>
        /// <param name="value">Value of criterion</param>
        public TResult Where<TValue>(Expression<Func<TEntity, TValue>> member, TValue value) {
            var criteria = GetCriteriaFrom(member, value);
            return ApplyFilterWith(criteria);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        public TResult Where<T1, T2>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2) {
            var criteria = GetCriteriaFrom(member_1, value_1, member_2, value_2);
            return ApplyFilterWith(criteria);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        /// <param name="member_3">Expression that defines name of criterion</param>
        /// <param name="value_3">Value of criterion</param>
        public TResult Where<T1, T2, T3>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2, Expression<Func<TEntity, T3>> member_3, T3 value_3) {
            var criteria = GetCriteriaFrom(member_1, value_1, member_2, value_2, member_3, value_3);
            return ApplyFilterWith(criteria);
        }


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        /// <param name="member_3">Expression that defines name of criterion</param>
        /// <param name="value_3">Value of criterion</param>
        /// <param name="member_4">Expression that defines name of criterion</param>
        /// <param name="value_4">Value of criterion</param>
        public TResult Where<T1, T2, T3, T4>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2, Expression<Func<TEntity, T3>> member_3, T3 value_3, Expression<Func<TEntity, T4>> member_4, T4 value_4) {
            var criteria = GetCriteriaFrom(member_1, value_1, member_2, value_2, member_3, value_3, member_4, value_4);
            return ApplyFilterWith(criteria);
        }

        #endregion
    }
}