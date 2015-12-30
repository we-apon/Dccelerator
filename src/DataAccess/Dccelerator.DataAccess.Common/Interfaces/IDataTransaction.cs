using System;
using System.Collections.Generic;


namespace Dccelerator.DataAccess {

    /// <summary>
    /// Abstraction of an 'transaction', in that scope allowed to inserting, updating or deleting entities.
    /// </summary>
    /// <example>
    /// <code>
    /// using(var transaction = dataManager.BeginTransaction()) {
    ///     transaction.Insert(something);
    ///     transaction.InsertMany(somethingElse);
    ///     transaction.Insert(somethingDiffering);
    ///     transaction.Update(other);
    ///     transaction.Delete(thatShit);
    ///     transaction.Execute(); /*this is an optional method, that executes transaction immidiatelly */
    /// } /* once it disposed, if transaction.Execute() was not called,*/
    /// /* information about that transaction will be stored, to lately make an bulk-insert or just executing it later */
    /// </code>
    /// </example>
    public interface IDataTransaction : IDisposable {

        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        void Insert<TEntity>( TEntity entity) where TEntity : class;


        /// <summary>
        /// Inserts <paramref name="entities"/> into database.
        /// </summary>
        void InsertMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        void Update<TEntity>( TEntity entity) where TEntity : class;


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        void UpdateMany<TEntity>( IEnumerable<TEntity> entities) where TEntity : class;


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        void Delete<TEntity>( TEntity entity) where TEntity : class;


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        void DeleteMany<TEntity>( IEnumerable<TEntity> entities) where TEntity : class;


        /// <summary>
        /// Immidiatelly executes all prepared actions of this transaction.
        /// If this method is not called, but transaction are disposed - all prepared actions will be performed later, in some scheduler.
        /// </summary>
        /// <returns>Result of performed transaction.</returns>
        bool Commit();


        /// <summary>
        /// Returns state of current transaction
        /// </summary>
        bool IsCommited { get; }
    }
}