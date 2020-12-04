using System;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Adapters.PostgreSql {
    public class PostgreSqlDataTransaction : SimpleScheduledTransaction {
        public PostgreSqlDataTransaction(ITransactionScheduler scheduler, IDataManagerAdoFactory factory, IsolationLevel isolationLevel) : base(scheduler, factory, isolationLevel) { }


        protected override bool IsDeadlockException(Exception exception) => exception.IsDeadlock();
    }
}