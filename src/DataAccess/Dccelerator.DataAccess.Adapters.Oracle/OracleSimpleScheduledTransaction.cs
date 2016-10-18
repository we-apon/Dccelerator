using System;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;
using Oracle.ManagedDataAccess.Client;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    static class DeadlockExceptionExtension {
        public static bool IsDeadLock(this Exception exception) {
            return (exception as OracleException)?.Number==00060;
        }
    }


    sealed class OracleSimpleScheduledTransaction : SimpleScheduledTransaction {
        public OracleSimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory, IsolationLevel isolationLevel) : base(scheduler, factory, isolationLevel) {}

        protected override bool IsDeadlockException(Exception exception) => exception.IsDeadLock();
    }
}