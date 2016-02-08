using System;
using System.Transactions;
using Dccelerator.DataAccess.Ado.Transactions;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    class SqlClientSimpleScheduledTransaction : SimpleScheduledTransaction {
        public SqlClientSimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory, IsolationLevel isolationLevel) : base(scheduler, factory, isolationLevel) {}


        class Scope : ISpecificTransactionScope {
            readonly TransactionScope _scope;


            public Scope(TransactionScope scope) {
                _scope = scope;
            }


            #region Implementation of IDisposable

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() {
                _scope.Dispose();
            }


            public void Complete() {
                _scope.Complete();
            }

            #endregion
        }

        #region Overrides of NotScheduledDataTransaction

        protected override bool IsDeadlockException(Exception exception) {
            throw new NotImplementedException();
        }


        protected override ISpecificTransactionScope BeginTransactionScope(IsolationLevel isolationLevel) {
            return new Scope(new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = (System.Transactions.IsolationLevel)isolationLevel}));
        }

        #endregion
    }
}