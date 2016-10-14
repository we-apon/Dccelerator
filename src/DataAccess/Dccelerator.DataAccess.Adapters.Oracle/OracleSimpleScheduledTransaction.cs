using System;
using System.Data.Common;
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

//
//        class Scope : ISpecificTransactionScope {
//            readonly OracleTransaction _transaction;
//
//            public Scope(OracleTransaction transaction) {
//                _transaction = transaction;
//            }
//
//            public void Dispose() => _transaction.Dispose();
//            public void Complete() => _transaction.Commit();
//            public void Rollback() => _transaction.Rollback();
//        }
//
//
//
//        protected override ISpecificTransactionScope BeginTransactionScope(IsolationLevel isolationLevel, DbConnection connection) {
//            var oracleConnection = (OracleConnection) connection;
//            var transaction = oracleConnection.BeginTransaction((System.Data.IsolationLevel) isolationLevel);
//            return new Scope(transaction);
//        }
    }
}