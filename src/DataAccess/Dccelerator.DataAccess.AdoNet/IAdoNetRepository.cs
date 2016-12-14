using System.Collections.Generic;
using System.Data.Common;


namespace Dccelerator.DataAccess.Ado {

    /// <summary>
    /// Interface of an repository that holds database-specific CRUD actions.
    /// </summary>
    public interface IAdoNetRepository {


        DbConnection GetConnection();

        /// <summary>
        /// Returns reader that can be used to get some data by <paramref name="entityName"/>, filtering it by <paramref name="criteria"/>.
        /// </summary>
        /// <param name="criteria">Filtering criteria</param>
        IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria);


        bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria);


        IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria);


        int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria);


        /// <summary>
        /// Inserts an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool Insert<TEntity>(IEntityInfo info, TEntity entity, DbActionArgs args) where TEntity : class;


        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities, DbActionArgs args) where TEntity : class;


        /// <summary>
        /// Updates an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool Update<TEntity>(IEntityInfo info, TEntity entity, DbActionArgs args) where TEntity : class;


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities, DbActionArgs args) where TEntity : class;


        /// <summary>
        /// Removes an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool Delete<TEntity>(IEntityInfo info, TEntity entity, DbActionArgs args) where TEntity : class;


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities, DbActionArgs args) where TEntity : class;
    }
}