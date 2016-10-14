using System;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Ado.SqlClient.Implementation {
    sealed class SqlClientSimpleScheduledTransaction : SimpleScheduledTransaction {
        public SqlClientSimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory, IsolationLevel isolationLevel) : base(scheduler, factory, isolationLevel) {}

        protected override bool IsDeadlockException(Exception exception) {
            return exception.IsDeadlock();
        }

//
//        class Scope : ISpecificTransactionScope {
//            readonly TransactionScope _scope;
//
//
//            public Scope(TransactionScope scope) {
//                _scope = scope;
//            }
//
//
//            #region Implementation of IDisposable
//
//            /// <summary>
//            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
//            /// </summary>
//            public void Dispose() {
//                _scope.Dispose();
//            }
//
//
//            public void Complete() {
//                _scope.Complete();
//            }
//
//            public void Rollback() {
//                /*do nothing*/
//            }
//
//            #endregion
//        }
//
//        protected override ISpecificTransactionScope BeginTransactionScope(IsolationLevel isolationLevel, DbConnection connection) {
//            return new Scope(new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = (System.Transactions.IsolationLevel)isolationLevel}));
//        }



    }
}