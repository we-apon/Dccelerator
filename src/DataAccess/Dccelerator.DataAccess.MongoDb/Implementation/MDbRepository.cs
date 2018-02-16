﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Lazy;
using Dccelerator.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;


namespace Dccelerator.DataAccess.MongoDb.Implementation {
    class MDbRepository : IMDbRepository {

        readonly TraceSource _trace = new TraceSource("Dccelerator.DataAccess.MongoDb");
        readonly ConnectionStringSettings _mongoConnectionString = ConfigurationManager.ConnectionStrings["MongoDb"];


        public IMongoDatabase MongoDatabase() {
            var mongoUrl = new MongoUrl(_mongoConnectionString.ConnectionString);
            return new MongoClient(_mongoConnectionString.ConnectionString).GetDatabase(mongoUrl.DatabaseName);
        }


        public KeyValuePair<string, string> KeyValuePairOf(object entity, IEntityInfo info) {
            if (entity is IIdentified<byte[]> bytesIdentified)
                return new KeyValuePair<string, string>(nameof(bytesIdentified.Id), bytesIdentified.Id.ToString());

            if (entity is IIdentified<Guid> guidIdentified)
                return new KeyValuePair<string, string>(nameof(bytesIdentified.Id), guidIdentified.Id.ToString());

            if (entity is IIdentified<long> longIdentified)
                return new KeyValuePair<string, string>(nameof(bytesIdentified.Id), longIdentified.Id.ToString());

            if (entity is IIdentified<int> intIdentified)
                return new KeyValuePair<string, string>(nameof(bytesIdentified.Id), intIdentified.ToString());

            throw new NotSupportedException("By default, supported only identifying of entities that implements at least one of the following interfaces:\n" +
                                            $"{nameof(IIdentified<byte[]>)},\n" +
                                            $"{nameof(IIdentified<Guid>)},\n" +
                                            $"{nameof(IIdentified<long>)},\n" +
                                            $"{nameof(IIdentified<int>)}.\n" +
                                            $"Please, implement at least one of these interfaces in entity '{info.EntityType.FullName}', or manually override that logic.");
        }


        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var collection = MongoDatabase().GetCollection<BsonDocument>(info.EntityName);
            IEnumerable<BsonDocument> results;
            if (!criteria.HasAny()) {
                results = collection.Find(new BsonDocument()).ToList();
                return results.Select(x => BsonSerializer.Deserialize<object>(x));
            }

            var filter = Builders<BsonDocument>.Filter.And(criteria.Select(x => x.Name == "Id" ? Builders<BsonDocument>.Filter.Eq("_id", x.Value) : Builders<BsonDocument>.Filter.Eq(x.Name, x.Value)));
            results = collection.Find(filter).ToList();
            return results.Select(x => BsonSerializer.Deserialize<object>(x));
        }


        public bool Insert<TEntity>(IEntityInfo info, TEntity entity, IMDbTransaction transaction) where TEntity : class {
            try {
                var collection = MongoDatabase().GetCollection<TEntity>(info.EntityName);
                collection.InsertOne(entity);

                return true;
            }
            catch (MongoDuplicateKeyException e) {
                _trace.TraceEvent(TraceEventType.Critical, 0, $"Duplicate key founded. Error on insert {info.EntityName} \n\n{e}");
                throw;
            }
            catch (Exception e) {
                transaction.Abort();
                _trace.TraceEvent(TraceEventType.Critical, 0, $"Error on insert {info.EntityName} \n\n{e}");
                throw;
            }
        }


        public bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            try {
                var collection = MongoDatabase().GetCollection<TEntity>(info.EntityName);
                collection.InsertMany(entities);

                return true;
            }
            catch (MongoDuplicateKeyException e) {
                _trace.TraceEvent(TraceEventType.Critical, 0, $"Duplicate key founded. Error on insert many {info.EntityName} \n\n{e}");
                throw;
            }
            catch (Exception e) {
                _trace.TraceEvent(TraceEventType.Critical, 0, $"Error on insert many {info.EntityName} \n\n{e}");
                throw;
            }
        }


        public bool Update<TEntity>(IEntityInfo info, TEntity entity, IMDbTransaction transaction) where TEntity : class {
            try {
                var collection = MongoDatabase().GetCollection<TEntity>(info.EntityName);
                var keyValuePair = KeyValuePairOf(entity, info);
                collection.UpdateOne(Builders<TEntity>.Filter.Eq(keyValuePair.Key, keyValuePair.Value), new ObjectUpdateDefinition<TEntity>(entity));

                return true;
            }
            catch (Exception e) {
                transaction.Abort();
                _trace.TraceEvent(TraceEventType.Critical, 0, $"Error on update {info.EntityName} \n\n{e}");
                throw;
            }
        }


        public bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            throw new NotImplementedException();
        }


        public bool Delete<TEntity>(IEntityInfo info, TEntity entity, IMDbTransaction transaction) where TEntity : class {
            try {
                var collection = MongoDatabase().GetCollection<TEntity>(info.EntityName);
                var keyValuePair = KeyValuePairOf(entity, info);
                collection.DeleteOne(Builders<TEntity>.Filter.Eq(keyValuePair.Key, keyValuePair.Value));

                return true;
            }
            catch (Exception e) {
                transaction.Abort();
                _trace.TraceEvent(TraceEventType.Critical, 0, $"Error on delete {info.EntityName} \n\n{e}");
                throw;
            }
        }


        public bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            throw new NotImplementedException();
        }


        public bool PerformInTransaction(IEnumerable<TransactionElement> elements) {
            using (var transaction = new MDbTransaction()) {
                transaction.Begin();

                foreach (var element in elements) {
                    transaction.StoreOrigin(element);

                    if (transaction.IsAborted)
                        return false;

                    switch (element.ActionType) {
                        case ActionType.Insert:
                            Insert(element.Info, element.Entity, transaction);
                            break;
                        case ActionType.Update:
                            Update(element.Info, element.Entity, transaction);
                            break;
                        case ActionType.Delete:
                            Delete(element.Info, element.Entity, transaction);
                            break;
                        default: throw new NotImplementedException();
                    }

                    transaction.CompleteAction(element);
                }

                transaction.Commit();
            }

            return true;
        }
    }
}