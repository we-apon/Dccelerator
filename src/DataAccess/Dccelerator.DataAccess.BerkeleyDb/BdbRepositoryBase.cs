using System;
using System.Collections.Generic;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public abstract class BdbRepositoryBase : IBDbRepository {
        
        protected abstract DatabaseEnvironment OpenEnvironment();

        protected abstract Database OpenReadOnlyPrimaryDb(string dbName, DatabaseEnvironment environment);

        protected abstract Database OpenPrimaryDb(string dbName, DatabaseEnvironment environment);


        protected abstract SecondaryDatabase OpenReadOnlySecondaryDb(Database primaryDb, string dbName, DatabaseEnvironment environment);

        protected abstract SecondaryDatabase OpenForeignKeyDatabase(Database primaryDb, Database foreignDb, BDbMapping mapping, DatabaseEnvironment environment);

        protected abstract DatabaseEntry KeyOf(object entity);

        protected abstract DatabaseEntry DataOf(object entity);

        #region Implementation of IBDbRepository

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


        public IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, string secondarySubName, DuplicatesPolicy duplicatesPolicy) {
            DatabaseEnvironment environment = null;
            Database primaryDb = null;
            SecondaryDatabase secondaryDb = null;
            Cursor cursor = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(entityName, environment);
                secondaryDb = OpenReadOnlySecondaryDb(primaryDb, secondarySubName, environment);

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


        public bool Insert(object entity, string name, ICollection<BDbMapping> mappings) {
            DatabaseEnvironment environment = null;
            Database primaryDb = null;
            IList<Database> foreignDatabases = new List<Database>(mappings.Count);
            IList<SecondaryDatabase> foreignKeyDatabases = new List<SecondaryDatabase>(mappings.Count);

            try {
                environment = OpenEnvironment();
                primaryDb = OpenPrimaryDb(name, environment);

                foreach (var mapping in mappings) {
                    if (mapping.Relationship != Relationship.ManyToOne)
                        continue;

                    var foreignDb = OpenReadOnlyPrimaryDb(mapping.Name, environment);
                    foreignDatabases.Add(foreignDb);

                    var foreignKey = OpenForeignKeyDatabase(primaryDb, foreignDb, mapping, environment);
                    foreignKeyDatabases.Add(foreignKey);
                }
                
                var key = KeyOf(entity);
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

        #endregion
    }
}