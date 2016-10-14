using System;
using System.Transactions;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Adapters.Oracle {

    internal static class DeadlockExceptionExtension {
        public static bool IsDeadLock(this Exception exception) {
            //BUG: Implement It!
            return false;
        }
    }


    //BUG: implement transaction scope!!
    class OracleSimpleScheduledTransaction : SimpleScheduledTransaction {
        public OracleSimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory,
            IsolationLevel isolationLevel) : base(scheduler, factory, isolationLevel) {}


        class Scope : ISpecificTransactionScope {
            readonly TransactionScope _scope;

            public Scope(TransactionScope scope) {
                _scope = scope;
            }

            public void Dispose() => _scope.Dispose();

            public void Complete() => _scope.Complete();
        }

        protected override bool IsDeadlockException(Exception exception) => exception.IsDeadLock();


        protected override ISpecificTransactionScope BeginTransactionScope(IsolationLevel isolationLevel) {
            return new Scope(new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = (System.Transactions.IsolationLevel) isolationLevel}));
        }
    }

}