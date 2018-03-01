using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dccelerator.DataAccess.Implementation;
using Dccelerator.DataAccess.MongoDb.Infrastructure;


namespace Dccelerator.DataAccess.MongoDb.Implementation
{
    public class MDbDataManagerFactory : IDataManagerMDbFactory
    {
        public IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new() {
            return new MDbDataGetter<TEntity>(CachedReadingRepository(), InfoAbout<TEntity>());
        }


        public IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new() {
            return new MDbDataGetter<TEntity>(ReadingRepository(), InfoAbout<TEntity>());
        }


        public IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            return new MDbDataExistenceChecker<TEntity>(CachedReadingRepository(), InfoAbout<TEntity>());
        }


        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            return new MDbDataExistenceChecker<TEntity>(ReadingRepository(), InfoAbout<TEntity>());
        }


        public IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        public IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel) {
            return new NotScheduledMDbTransaction(this);
        }


        public ITransactionScheduler Scheduler() {
            return new DummyScheduler();
        }


        public IEntityInfo InfoAbout<TEntity>() {
            return MDbInfoAbout<TEntity>();
        }


        public IMdbEntityInfo MDbInfoAbout<TEntity>() {
            var info = MdbInfoAbout<TEntity>.Info;
            if (info.Repository == null)
                info.Repository = Repository();

            return info;
        }


        public IEntityInfo InfoAbout(Type entityType) {
            var info = new MdbInfoAboutEntity(entityType).Info;
            if (info.Repository == null)
                info.Repository = Repository();

            return info;
        }

        
        public IReadingRepository ReadingRepository() {
           return new MDbReadingRepository();
        }


        public IReadingRepository CachedReadingRepository() {
            return new MDbCachedReadingRepository();
        }

        public IMDbRepository Repository() {
            return new MDbRepository();
        }
    }
}
