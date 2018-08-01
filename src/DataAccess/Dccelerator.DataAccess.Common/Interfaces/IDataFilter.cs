using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace Dccelerator.DataAccess {
    /// <summary>
    /// An interface for filtering entities with some criteria
    /// </summary>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TResult">Type of result of filtering</typeparam>
    public interface IDataFilter<TEntity, TResult> where TEntity : class {

        /// <summary>
        /// Returns all <typeparamref name="TEntity"/> what it can get
        /// </summary>
        TResult All();


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// Criteria will be getted from <paramref name="context"/>
        /// </summary>
        /// <param name="context">Context of <paramref name="expressions"/></param>
        /// <param name="expressions">Expression that selects filtering criteria</param>
        /// <returns><typeparamref name="TEntity"/> filtered with specified criteria.</returns>
        TResult By<TContext>(TContext context, params Expression<Func<TContext, object>>[] expressions);



        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with <paramref name="criteria"/>.
        /// </summary>
        TResult Where(params KeyValuePair<string, object>[] criteria);


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with <paramref name="criteria"/>.
        /// </summary>
        TResult Where(params KeyValuePair<Expression<Func<TEntity, object>>, object>[] criteria);


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member">Expression that defines name of criterion</param>
        /// <param name="value">Value of criterion</param>
        TResult Where<TValue>( Expression<Func<TEntity, TValue>> member, TValue value);


        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        TResult Where<T1, T2>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2);




        /// <summary>
        /// Applies filtering to <typeparamref name="TEntity"/> specified with criteria.
        /// </summary>
        /// <param name="member_1">Expression that defines name of criterion</param>
        /// <param name="value_1">Value of criterion</param>
        /// <param name="member_2">Expression that defines name of criterion</param>
        /// <param name="value_2">Value of criterion</param>
        /// <param name="member_3">Expression that defines name of criterion</param>
        /// <param name="value_3">Value of criterion</param>
        TResult Where<T1, T2, T3>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3);



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
        TResult Where<T1, T2, T3, T4>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3,
            Expression<Func<TEntity, T4>> member_4, T4 value_4);

    }
}