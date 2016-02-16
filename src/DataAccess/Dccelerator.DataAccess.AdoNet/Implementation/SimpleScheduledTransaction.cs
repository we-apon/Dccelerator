namespace Dccelerator.DataAccess.Ado.Implementation {
    public abstract class SimpleScheduledTransaction : NotScheduledDataTransaction {
        readonly ITransactionScheduler _scheduler;


        protected SimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory, IsolationLevel isolationLevel) : base(factory, isolationLevel) {
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