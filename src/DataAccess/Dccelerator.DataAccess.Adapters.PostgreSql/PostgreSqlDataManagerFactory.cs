using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Adapters.PostgreSql
{
    public abstract class PostgreSqlDataManagerFactory<TRepository> : DataManagerAdoFactoryBase<PostgreSqlEntityInfo<TRepository>> where TRepository : class, IAdoNetRepository
    {
        protected override DirectReadingRepository NotCachedReadingRepository<TEntity>() {
            return new PostgreSqlDirectReadingRepository();
        }


        public override IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            return new PostgreSqlDataTransaction(scheduler, this, isolationLevel);
        }


        public override IReadingRepository ReadingRepository() {
            return new PostgreSqlForcedCacheReadingRepository();
        }


        public override IAdoNetRepository AdoNetRepository() {
            throw new NotImplementedException();
        }


        protected override PostgreSqlEntityInfo<TRepository> GetEntityInfo(Type type) {
            throw new NotImplementedException();
        }
    }
}
