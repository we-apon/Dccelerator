using System;
using System.Collections.Generic;
using BerkeleyDB;
using Dccelerator.DataAccess.Attributes;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public abstract class BDbRepositoryBase : IBDbRepository {
        readonly IBDbSchema _schema;


        protected BDbRepositoryBase(IBDbSchema schema) {
            _schema = schema;
        }

        protected abstract DatabaseEntry KeyOf(object entity, IBDbEntityInfo info);


        protected virtual DatabaseEntry DataOf(object entity) {
            if (entity == null)
                return null;

            return new DatabaseEntry(entity.ToBinnary());
        }

        protected virtual string SecondaryDbName(Database primaryDb, string indexSubName) {
            return $"{primaryDb.DatabaseName}-->{indexSubName}";
        }

        protected virtual string ForeignKeyDbName(Database primaryDb, Database foreignDb) {
            return $"{primaryDb.DatabaseName}-->{foreignDb.DatabaseName}";
        }


        protected virtual void Insert(TransactionElement element, Database primaryDb, Transaction transaction) {
            var key = KeyOf(element.Entity, element.Info);
            var data = DataOf(element.Entity);
            primaryDb.PutNoOverwrite(key, data, transaction);
        }

        protected virtual void Update(TransactionElement element, Database primaryDb, Transaction transaction) {
            var key = KeyOf(element.Entity, element.Info);
            var data = DataOf(element.Entity);
            primaryDb.Put(key, data, transaction);
        }

        protected virtual void Delete(TransactionElement element, Database primaryDb, Transaction transaction) {
            var key = KeyOf(element.Entity, element.Info);
            primaryDb.Delete(key, transaction);
        }


        protected virtual Transaction BeginTransaction(DatabaseEnvironment environment) {
            return environment.BeginTransaction();
        }


        

        #region Implementation of IBDbRepository

        public abstract bool IsPrimaryKey(IDataCriterion criterion);


        public virtual DatabaseEntry EntryFrom(IDataCriterion criterion) {
            return new DatabaseEntry(criterion.Value.ToBinnary());
        }


        public IEnumerable<DatabaseEntry> ContinuouslyReadToEnd(string entityName) {
            Cursor cursor = null;
            try {
                var primaryDb = _schema.GetPrimaryDb(entityName, readOnly: true);

                cursor = primaryDb.Cursor();
                while (cursor.MoveNext())
                    yield return cursor.Current.Value;
            }
            finally {
                cursor?.Close();
            }
        }


        public IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName) {
            var primaryDb = _schema.GetPrimaryDb(entityName, readOnly: true);
            if (primaryDb.Exists(key))
                yield return primaryDb.Get(key).Value;
        }


        public IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, string indexSubName, DuplicatesPolicy duplicatesPolicy) {
            Cursor cursor = null;
            try {
                var primaryDb = _schema.GetPrimaryDb(entityName, readOnly: true);
                var secondaryDb = _schema.GetReadOnlySecondaryDb(primaryDb, indexSubName, duplicatesPolicy);

                if (duplicatesPolicy == DuplicatesPolicy.NONE) {
                    if (secondaryDb.Exists(key))
                        yield return secondaryDb.Get(key).Value;
                }

                cursor = secondaryDb.Cursor();
                if (!cursor.Move(key, exact:true))
                    yield break;

                yield return cursor.Current.Value;

                while (cursor.MoveNextDuplicate())
                    yield return cursor.Current.Value;
            }
            finally {
                cursor?.Close();
            }
        }


        public IEnumerable<DatabaseEntry> GetByJoin(string entityName, ICollection<IDataCriterion> criteria, IBDbEntityInfo info) {

            var cursorsLength = 0;
            var secondaryCursors = new SecondaryCursor[criteria.Count];
            JoinCursor joinCursor = null;

            try {
                var primaryDb = _schema.GetPrimaryDb(entityName, readOnly: true);

                foreach (var criterion in criteria) {
                    ForeignKeyAttribute foreignKeyMapping;
                    var duplicatesPolicy = info.ForeignKeys.TryGetValue(criterion.Name, out foreignKeyMapping)
                        ? foreignKeyMapping.DuplicatesPolicy
                        : DefaultDuplicatesPolicy;

                    var secondaryDb = _schema.GetReadOnlySecondaryDb(primaryDb, criterion.Name, duplicatesPolicy);

                    var cursor = secondaryDb.SecondaryCursor();
                    secondaryCursors[cursorsLength++] = cursor;

                    var key = new DatabaseEntry(criterion.Value.ToBinnary());
                    if (!cursor.Move(key, true))
                        yield break;
                }

                joinCursor = primaryDb.Join(secondaryCursors, true);
                while (joinCursor.MoveNext())
                    yield return joinCursor.Current.Value;
            }
            finally {
                joinCursor?.Close();

                for (var i = 0; i < cursorsLength; i++) {
                    secondaryCursors[i].Close();
                }
            }
        }


        protected virtual DuplicatesPolicy DefaultDuplicatesPolicy => DuplicatesPolicy.UNSORTED;

        
        public bool PerformInTransaction(ICollection<IBDbEntityInfo> entityInfos, IEnumerable<TransactionElement> elements) {

            Transaction transaction = null;

            try {

                transaction = _schema.BeginTransactionFor(entityInfos);
                
                foreach (var element in elements) {
                    var primaryDb = _schema.GetPrimaryDb(element.Info.EntityName, readOnly: false);

                    switch (element.ActionType) {
                        case ActionType.Insert:
                            Insert(element, primaryDb, transaction);
                            break;

                        case ActionType.Update:
                            Update(element, primaryDb, transaction);
                            break;

                        case ActionType.Delete:
                            Delete(element, primaryDb, transaction);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
                
                transaction.Commit();
                return true;
            }
            catch (Exception e) {
                //todo: write log

                transaction?.Abort();
                return false;
            }
        }

        #endregion
    }
}