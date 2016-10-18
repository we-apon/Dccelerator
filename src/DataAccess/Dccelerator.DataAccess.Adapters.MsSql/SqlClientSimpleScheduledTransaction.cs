using System;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Ado.SqlClient {
    sealed class SqlClientSimpleScheduledTransaction : SimpleScheduledTransaction {
        public SqlClientSimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory, IsolationLevel isolationLevel) : base(scheduler, factory, isolationLevel) {}

        protected override bool IsDeadlockException(Exception exception) {
            return exception.IsDeadlock();
        }
    }
}