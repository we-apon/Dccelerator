using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dccelerator.DataAccess.Implementations.DataGetters;


namespace Dccelerator.DataAccess
{
    public class Get<TEntity> where TEntity : class, new()
    {
        internal static readonly ConfigurationRespectfullDataGetter<TEntity> Getter = new ConfigurationRespectfullDataGetter<TEntity>(); 

        public static IDataSelector<TEntity, TValue> Select<TValue>(Expression<Func<TEntity, TValue>> member) {
            return Getter.Select(member);
        }


        public static IEnumerable<TEntity> All() {
            return Getter.All();
        }


        public static IEnumerable<TEntity> By<TContext>(TContext context, params Expression<Func<TContext, object>>[] expressions) {
            return Getter.By(context, expressions);
        }


        public static IEnumerable<TEntity> Where(params KeyValuePair<Expression<Func<TEntity, object>>, object>[] criteria) {
            return Getter.Where(criteria);
        }


        public static IEnumerable<TEntity> Where(params KeyValuePair<string, object>[] criteria) {
            return Getter.Where(criteria);
        }



        public static IEnumerable<TEntity> Where<TValue>(Expression<Func<TEntity, TValue>> member, TValue value) {
            return Getter.Where(member, value);
        }



        public static IEnumerable<TEntity> Where<T1, T2>(Expression<Func<TEntity, T1>> member_1, T1 value_1, Expression<Func<TEntity, T2>> member_2, T2 value_2) {
            return Getter.Where(member_1, value_1, member_2, value_2);
        }


        public static IEnumerable<TEntity> Where<T1, T2, T3>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3) {

            return Getter.Where(member_1, value_1, member_2, value_2, member_3, value_3);
        }


        public static IEnumerable<TEntity> Where<T1, T2, T3, T4>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3,
            Expression<Func<TEntity, T4>> member_4, T4 value_4) {

            return Getter.Where(member_1, value_1, member_2, value_2, member_3, value_3, member_4, value_4);
        }


        
    }

}