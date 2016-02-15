namespace Dccelerator.DataAccess {

    /// <summary>
    /// An scheduler of transactions.
    /// </summary>
    public interface ITransactionScheduler {

        /// <summary>
        /// Commits every transaction in order that they was created.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool SequentialCommit();

        /// <summary>
        /// Groups entities of every prepared transaction by their type and uses bulk-inserts, group-updates and group-deleting.
        /// This method should works much faster, than <see cref="SequentialCommit"/>, but if some transaction fails - rollbacks and retries will be much slower.
        /// </summary>
        /// <returns>Result of operation</returns>
        bool GroupingCommit();


        /// <summary>
        /// Appends an transaction into scheduler.
        /// This method mostly shall not be used in client code.
        /// </summary>
        void Append(IDataTransaction transaction);
    }
}