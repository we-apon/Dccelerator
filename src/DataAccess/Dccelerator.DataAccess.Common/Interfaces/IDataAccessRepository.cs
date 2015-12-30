using System.Collections.Generic;
using System.Data.Common;


namespace Dccelerator.DataAccess {

    /// <summary>
    /// Interface of an repository that holds database-specific CRUD actions.
    /// </summary>
    public interface IDataAccessRepository {

        /// <summary>
        /// Returns reader that can be used to get some data by <paramref name="entityName"/>, filtering it by <paramref name="criteria"/>.
        /// </summary>
        /// <param name="entityName">Database-specific name of some entity</param>
        /// <param name="criteria">Filtering criteria</param>
        /// <param name="reader">An data reader</param>
        /// <returns>Connection of <paramref name="reader"/>. Reader and connection will be disposed just after all requested information are readed.</returns>
        DbConnection Read( string entityName,  ICollection<IDataCriterion> criteria, out DbDataReader reader);


        /// <summary>
        /// Inserts an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool Insert<TEntity>( string entityName,  TEntity entity) where TEntity : class;


        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool InsertMany<TEntity>( string entityName,  IEnumerable<TEntity> entities) where TEntity : class;


        /// <summary>
        /// Updates an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool Update<TEntity>( string entityName,  TEntity entity) where TEntity : class;


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool UpdateMany<TEntity>( string entityName,  IEnumerable<TEntity> entities) where TEntity : class;


        /// <summary>
        /// Removes an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool Delete<TEntity>( string entityName,  TEntity entity) where TEntity : class;


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool DeleteMany<TEntity>( string entityName,  IEnumerable<TEntity> entities) where TEntity : class;
    }
}