using System;
using System.Collections.Generic;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public abstract class BDbRepositoryBase : IBDbRepository {
        
        protected abstract DatabaseEnvironment OpenEnvironment();

        protected abstract Database OpenReadOnlyPrimaryDb(string dbName, DatabaseEnvironment environment);

        protected abstract Database OpenPrimaryDb(string dbName, DatabaseEnvironment environment);


        protected abstract SecondaryDatabase OpenReadOnlySecondaryDb(Database primaryDb, string indexSubName, DatabaseEnvironment environment, DuplicatesPolicy duplicatesPolicy);

        protected abstract SecondaryDatabase OpenForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute mapping, DatabaseEnvironment environment);

        protected abstract DatabaseEntry KeyOf(object entity, IBDbEntityInfo info);

        protected abstract DatabaseEntry DataOf(object entity);

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
            DatabaseEnvironment environment = null;
            Database primaryDb = null;
            Cursor cursor = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(entityName, environment);

                cursor = primaryDb.Cursor();
                while (cursor.MoveNext())
                    yield return cursor.Current.Value;
            }
            finally {
                if (cursor != null) {
                    cursor.Close();
                    cursor.Dispose();
                }
                
                if (primaryDb != null) {
                    primaryDb.Close(true);
                    primaryDb.Dispose();
                }

                environment?.Close();
            }
        }


        public IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName) {
            DatabaseEnvironment environment = null;
            Database primaryDb = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(entityName, environment);

                if (primaryDb.Exists(key))
                    yield return primaryDb.Get(key).Value;

            }
            finally {
                if (primaryDb != null) {
                    primaryDb.Close(true);
                    primaryDb.Dispose();
                }

                environment?.Close();
            }
        }


        public IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, string indexSubName, DuplicatesPolicy duplicatesPolicy) {
            DatabaseEnvironment environment = null;
            Database primaryDb = null;
            SecondaryDatabase secondaryDb = null;
            Cursor cursor = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(entityName, environment);
                secondaryDb = OpenReadOnlySecondaryDb(primaryDb, indexSubName, environment, duplicatesPolicy);

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
                if (cursor != null) {
                    cursor.Close();
                    cursor.Dispose();
                }

                if (secondaryDb != null) {
                    secondaryDb.Close(true);
                    secondaryDb.Dispose();
                }

                if (primaryDb != null) {
                    primaryDb.Close(true);
                    primaryDb.Dispose();
                }

                environment?.Close();
            }
        }


        public IEnumerable<DatabaseEntry> GetByJoin(string entityName, ICollection<IDataCriterion> criteria, IBDbEntityInfo info) {
            DatabaseEnvironment environment = null;
            Database primaryDb = null;

            var dbsLength = 0;
            var cursorsLength = 0;

            var secondaryDbs = new SecondaryDatabase[criteria.Count];
            var secondaryCursors = new SecondaryCursor[criteria.Count];
            JoinCursor joinCursor = null;

            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(entityName, environment);

                foreach (var criterion in criteria) {
                    ForeignKeyAttribute foreignKeyMapping;
                    var duplicatesPolicy = info.ForeignKeys.TryGetValue(criterion.Name, out foreignKeyMapping)
                        ? foreignKeyMapping.DuplicatesPolicy
                        : DefaulDuplicatesPolicy;

                    var secondaryDb = OpenReadOnlySecondaryDb(primaryDb, criterion.Name, environment, duplicatesPolicy);
                    secondaryDbs[dbsLength++] = secondaryDb;

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
                if (joinCursor != null) {
                    joinCursor.Close();
                    joinCursor.Dispose();
                }

                for (var i = 0; i < cursorsLength; i++) {
                    secondaryCursors[i].Close();
                    secondaryCursors[i].Dispose();
                }

                for (var i = 0; i < dbsLength; i++) {
                    secondaryDbs[i].Close(true);
                    secondaryDbs[i].Dispose();
                }

                if (primaryDb != null) {
                    primaryDb.Close(true);
                    primaryDb.Dispose();
                }

                environment?.Close();
            }
        }


        protected virtual DuplicatesPolicy DefaulDuplicatesPolicy => DuplicatesPolicy.UNSORTED;


/*

        public bool Insert(object entity, IBDbEntityInfo info) {
            DatabaseEnvironment environment = null;
            Database primaryDb = null;
            IList<Database> foreignDatabases = new List<Database>(info.ForeignKeys.Count);
            IList<SecondaryDatabase> foreignKeyDatabases = new List<SecondaryDatabase>(info.ForeignKeys.Count);

            try {
                environment = OpenEnvironment();
                primaryDb = OpenPrimaryDb(info.EntityName, environment);

                foreach (var foreignKeyMapping in info.ForeignKeys.Values) {
                    if (foreignKeyMapping.Relationship != Relationship.ManyToOne)
                        continue;

                    var foreignDb = OpenReadOnlyPrimaryDb(foreignKeyMapping.ForeignEntityName, environment);
                    foreignDatabases.Add(foreignDb);

                    var foreignKey = OpenForeignKeyDatabase(primaryDb, foreignDb, foreignKeyMapping, environment);
                    foreignKeyDatabases.Add(foreignKey);
                }
                
                var key = KeyOf(entity, info);
                var data = DataOf(entity);

                primaryDb.PutNoOverwrite(key, data);
                return true;
            }
            catch (Exception e) {
                //todo: write log
                return false;
            }
            finally {
                foreach (var foreignKeyDatabase in foreignKeyDatabases) {
                    foreignKeyDatabase.Close(true);
                    foreignKeyDatabase.Dispose();
                }

                foreach (var foreignDatabase in foreignDatabases) {
                    foreignDatabase.Close(true);
                    foreignDatabase.Dispose();
                }

                if (primaryDb != null) {
                    primaryDb.Close(true);
                    primaryDb.Dispose();
                }

                environment?.Close();
            }
        }
*/


        public bool PerformInTransaction(IEnumerable<TransactionElement> elements) {
            DatabaseEnvironment environment = null;

            Transaction transaction = null;

            var primaryDatabases = new Dictionary<string, Database>();
            var foreignDatabases = new Dictionary<string, Database>();
            var foreignKeyDatabases = new Dictionary<string, SecondaryDatabase>();

            var performedCount = 0;

            try {
                environment = OpenEnvironment();

                transaction = BeginTransaction(environment);

                foreach (var element in elements) {
                    Database primaryDb;

                    if (!primaryDatabases.TryGetValue(element.Info.EntityName, out primaryDb)) {
                        primaryDb = OpenPrimaryDb(element.Info.EntityName, environment);
                        primaryDatabases.Add(element.Info.EntityName, primaryDb);
                    }


                    foreach (var keyMapping in element.Info.ForeignKeys.Values) {
                        Database foreignDb;

                        if (!foreignDatabases.TryGetValue(keyMapping.ForeignEntityName, out foreignDb)) {
                            foreignDb = OpenPrimaryDb(keyMapping.ForeignEntityName, environment);
                            foreignDatabases.Add(keyMapping.ForeignEntityName, foreignDb);
                        }

                        SecondaryDatabase keyDatabase;
                        var foreignKeyDbName = ForeignKeyDbName(primaryDb, foreignDb);
                        if (!foreignKeyDatabases.TryGetValue(foreignKeyDbName, out keyDatabase)) {
                            keyDatabase = OpenForeignKeyDatabase(primaryDb, foreignDb, keyMapping, environment);
                            foreignKeyDatabases.Add(foreignKeyDbName, keyDatabase);
                        }
                    }


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

                    performedCount++;
                }
                
                transaction.Commit();
                return true;
            }
            catch (Exception e) {
                //todo: write log

                transaction.Abort();
                return false;
            }
            finally {

                foreach (var foreignKeyDatabase in foreignKeyDatabases) {
                    foreignKeyDatabase.Value.Close(true);
                    foreignKeyDatabase.Value.Dispose();
                }

                foreach (var foreignDatabase in foreignDatabases) {
                    foreignDatabase.Value.Close(true);
                    foreignDatabase.Value.Dispose();
                }

                foreach (var primaryDatabase in primaryDatabases) {
                    primaryDatabase.Value.Close(true);
                    primaryDatabase.Value.Dispose();
                }

                environment?.Close();
            }

        }

        #endregion
    }
}