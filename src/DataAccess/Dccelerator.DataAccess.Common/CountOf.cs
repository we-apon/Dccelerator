using System;
using System.Linq.Expressions;
using Dccelerator.DataAccess.Implementations;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess
{
    
    public class CountOf<TEntity> where TEntity : class, new()
    {

        private static readonly IDataCountChecker<TEntity> _checker = new DataCountChecker<TEntity>(ConfigurationOf<TEntity>.GetReadingRepository());


        public static long All() {
            return _checker.All();
        }


        public static long Where<TValue>(Expression<Func<TEntity, TValue>> member, TValue value) {
            return _checker.Where(member, value);
        }


        public static long Where<T1, T2>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2) {

            return _checker.Where(member_1, value_1, member_2, value_2);
        }


        public static long Where<T1, T2, T3>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3) {
            
            return _checker.Where(member_1, value_1, member_2, value_2, member_3, value_3);
        }


        public static long Where<T1, T2, T3, T4>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1,
            Expression<Func<TEntity, T2>> member_2, T2 value_2,
            Expression<Func<TEntity, T3>> member_3, T3 value_3,
            Expression<Func<TEntity, T4>> member_4, T4 value_4) {
            
            return _checker.Where(member_1, value_1, member_2, value_2, member_3, value_3, member_4, value_4);
        }



    }

}