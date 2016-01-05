using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dccelerator.DataAccess.Entities;
using Dccelerator.DataAccess.Implementations.DataGetters;


namespace Dccelerator.DataAccess
{
    
    public class GetRandom<TEntity> where TEntity : class, new()
    {
        private static readonly ForcedCacheDataGetter<TEntity> _getter = new ForcedCacheDataGetter<TEntity>();

        public static TEntity Where(params Func<TEntity, bool>[] criteria) {
            return RandomEntityFrom(_getter.All(), criteria);
        }


        public static TEntity Where<TValue>(Expression<Func<TEntity, TValue>> member, TValue value, params Func<TEntity, bool>[] criteria) {
            return RandomEntityFrom(_getter.Where(member, value), criteria);
        }


        public static TEntity Where<T1, T2>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2, 
            params Func<TEntity, bool>[] criteria) {

            return RandomEntityFrom(_getter.Where(member_1, value_1, member_2, value_2), criteria);
        }

        
        public static TEntity Where<T1, T2, T3>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2, 
            Expression<Func<TEntity, T3>> member_3, T3 value_3, 
            params Func<TEntity, bool>[] criteria) {

            return RandomEntityFrom(_getter.Where(member_1, value_1, member_2, value_2, member_3, value_3), criteria);
        }


        public static TEntity Where<T1, T2, T3, T4>(
            Expression<Func<TEntity, T1>> member_1, T1 value_1, 
            Expression<Func<TEntity, T2>> member_2, T2 value_2, 
            Expression<Func<TEntity, T3>> member_3, T3 value_3, 
            Expression<Func<TEntity, T4>> member_4, T4 value_4, 
            params Func<TEntity, bool>[] criteria) {

            return RandomEntityFrom(_getter.Where(member_1, value_1, member_2, value_2, member_3, value_3, member_4, value_4), criteria);
        }

        


        public static TEntity Any() {
            return Where();
        }



        static TEntity RandomEntityFrom(IEnumerable<object> entities, Func<TEntity, bool>[] criteria) {
            return entities.Shuffle().Cast<TEntity>().Perform(AllowLazyLoading).FirstOrDefault(x => criteria.All(z => z(x)));
        }


        static void AllowLazyLoading( TEntity obj) {
            var entity = obj as Entity;
            entity?.Internal_AllowLazyLoading(_getter.Repository);
        }



    }

}