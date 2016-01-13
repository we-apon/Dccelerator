using System.Data;


namespace Dccelerator.DataAccess.Implementations.Transactions {
    public abstract class SimpleScheduledTransaction : NotScheduledDataTransaction {
        readonly ITransactionScheduler _scheduler;


        public SimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerFactory factory, IsolationLevel isolationLevel) : base(factory, isolationLevel) {
            _scheduler = scheduler;
        }


        #region Overrides of NotScheduledDataTransaction

        /// <summary>
        /// Doing nothing.
        /// </summary>
        public override void Dispose() {
            if (IsCommited)
                return;

            _scheduler.Append(this); //todo: this is will be work, but that is bad way.
        }

        #endregion
    }
}