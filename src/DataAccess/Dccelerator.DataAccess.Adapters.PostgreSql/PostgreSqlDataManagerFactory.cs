using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Adapters.PostgreSql
{
    public class PostgreSqlDataManagerFactory<TRepository> : DataManagerAdoFactoryBase<PostgreSqlEntityInfo<TRepository>> where TRepository : class, IAdoNetRepository
    {
        protected override DirectReadingRepository NotCachedReadingRepository<TEntity>() {
            throw new NotImplementedException();
        }


        public override IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) {
            throw new NotImplementedException();
        }


        public override IReadingRepository ReadingRepository() {
            throw new NotImplementedException();
        }


        public override IAdoNetRepository AdoNetRepository() {
            throw new NotImplementedException();
        }


        protected override PostgreSqlEntityInfo<TRepository> GetEntityInfo(Type type) {
            throw new NotImplementedException();
        }
    }
}
