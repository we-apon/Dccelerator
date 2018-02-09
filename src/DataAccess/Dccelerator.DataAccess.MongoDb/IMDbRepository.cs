using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dccelerator.DataAccess.MongoDb.Implementation;
using MongoDB.Driver;


namespace Dccelerator.DataAccess.MongoDb
{
    public interface IMDbRepository {
        IMongoDatabase MongoDatabase();

        IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria);

        
        bool Insert<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class;


       
        bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class;


        
        bool Update<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class;


        
        bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class;


        
        bool Delete<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class;


        
        bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class;


        bool PerformInTransaction(ICollection<IMdbEntityInfo> entityInfos, IEnumerable<TransactionElement> elements);
    }
}
