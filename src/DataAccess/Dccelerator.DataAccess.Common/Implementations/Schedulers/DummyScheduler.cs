using System.Collections.Generic;
using System.Threading.Tasks;


namespace Dccelerator.DataAccess.Implementations.Schedulers {
    public class DummyScheduler : ITransactionScheduler {

        private readonly List<IDataTransaction> _transactions = new List<IDataTransaction>();
        private readonly object _lock = new object();

        #region Implementation of ITransactionScheduler

        /// <summary>
        /// Commits every transaction in order that they was created.
        /// </summary>
        /// <returns>Result of operation</returns>
        public bool SequentialCommit() {
            IDataTransaction[] preparedTransactions;
            lock (_lock) {
                preparedTransactions = _transactions.ToArray();
                _transactions.Clear();
            }

            var result = true;
            Parallel.ForEach(preparedTransactions, transaction => {
                if (transaction.Commit())
                    return;

                result = false;
                Append(transaction);
            });

            return result;
        }


        /// <summary>
        /// Just calls <see cref="SequentialCommit"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public bool GroupingCommit() {
            return SequentialCommit();
        }


        /// <summary>
        /// Appends an transaction into scheduler.
        /// This method mostly shall not be used in client code.
        /// </summary>
        public void Append(IDataTransaction transaction) {
            lock (_lock)
                _transactions.Add(transaction);
        }

        #endregion
    }
}