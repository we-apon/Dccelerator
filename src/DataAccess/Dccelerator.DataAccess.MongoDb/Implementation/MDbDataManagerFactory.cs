using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Dccelerator.DataAccess.MongoDb.Implementation
{
    class MDbDataManagerFactory : IDataManagerMDbFactory
    {
        public IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new() {
            return NotCachedGetterFor<TEntity>();
        }


        public IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new() {
            return new MDbDataGetter<TEntity>(ReadingRepository(), InfoAbout<TEntity>());
        }


        public IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        public IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        public IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel) {
            throw new NotImplementedException();
        }


        public ITransactionScheduler Scheduler() {
            throw new NotImplementedException();
        }


        public IEntityInfo InfoAbout<TEntity>() {
            throw new NotImplementedException();
        }


        public IEntityInfo InfoAbout(Type entityType) {
            throw new NotImplementedException();
        }


        public IReadingRepository ReadingRepository() {
           return new MDbReadingRepository();
        }


        public IMDbRepository Repository() {
            return new MDbRepository();
        }
    }
}
