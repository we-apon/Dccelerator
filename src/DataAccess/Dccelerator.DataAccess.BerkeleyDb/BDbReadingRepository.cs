using System;
using System.Collections.Generic;
using System.Linq;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbReadingRepository : IInternalReadingRepository {
        readonly string _environmentPath;
        readonly string _dbFilePath;
        readonly string _password;


        public BDbReadingRepository(string environmentPath, string dbFilePath, string password) {
            _environmentPath = environmentPath;
            _dbFilePath = dbFilePath;
            _password = password;
        }


        DatabaseEnvironment OpenEnvironment() {
            var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
                SystemMemory = true,
                /*Lockdown = true*/
            };
            databaseEnvironmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
            var environment = DatabaseEnvironment.Open(_environmentPath, databaseEnvironmentConfig);
            return environment;
        }


        static BTreeDatabase OpenReadOnlyPrimaryDb(string filePath, string dbName, DatabaseEnvironment environment) {
            return BTreeDatabase.Open(filePath, dbName, new BTreeDatabaseConfig {
                Env = environment,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Creation = CreatePolicy.NEVER,
                ReadOnly = true,
            });
        }


        static SecondaryBTreeDatabase OpenReadOnlySecondaryDb(BTreeDatabase primaryDb, string filePath, string dbName, DatabaseEnvironment environment) {
            return SecondaryBTreeDatabase.Open(filePath, $"{primaryDb.DatabaseName}.{dbName}", new SecondaryBTreeDatabaseConfig(primaryDb, (key, data) => null) {
                Env = environment,
                ReadOnly = true,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Duplicates = DuplicatesPolicy.UNSORTED,
                Creation = CreatePolicy.NEVER
            });
        }


        IEnumerable<DatabaseEntry> ContinuouslyReadToEnd(string entityName) {
            DatabaseEnvironment environment = null;
            BTreeDatabase primaryDb = null;
            BTreeCursor cursor = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(_dbFilePath, entityName, environment);

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


        IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName) {
            DatabaseEnvironment environment = null;
            BTreeDatabase primaryDb = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(_dbFilePath, entityName, environment);

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


        IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, string secondarySubName) {
            DatabaseEnvironment environment = null;
            BTreeDatabase primaryDb = null;
            SecondaryBTreeDatabase secondaryDb = null;
            Cursor cursor = null;
            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(_dbFilePath, entityName, environment);
                secondaryDb = OpenReadOnlySecondaryDb(primaryDb, _dbFilePath, secondarySubName, environment);

                if (secondaryDb.Duplicates == DuplicatesPolicy.NONE) {
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
        


        IEnumerable<DatabaseEntry> GetByJoin(string entityName, ICollection<IDataCriterion> criteria) {
            DatabaseEnvironment environment = null;
            BTreeDatabase primaryDb = null;

            var length = 0;

            var secondaryDbs = new SecondaryBTreeDatabase[criteria.Count];
            var secondaryCursors = new SecondaryCursor[criteria.Count];
            JoinCursor joinCursor = null;

            try {
                environment = OpenEnvironment();
                primaryDb = OpenReadOnlyPrimaryDb(_dbFilePath, entityName, environment);

                foreach (var criterion in criteria) {
                    var secondaryDb = OpenReadOnlySecondaryDb(primaryDb, _dbFilePath, criterion.Name, environment);
                    var cursor = secondaryDb.SecondaryCursor();

                    secondaryDbs[length] = secondaryDb;
                    secondaryCursors[length++] = cursor;

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

                for (var i = 0; i < length; i++) {
                    secondaryCursors[i].Close();
                    secondaryCursors[i].Dispose();

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

        IEnumerable<DatabaseEntry> GetEntriesFor(string entityName, ICollection<IDataCriterion> criteria) {
            if (criteria.Count == 0)
                return ContinuouslyReadToEnd(entityName);

            if (criteria.Count == 1) {
                var criterion = criteria.First();
                var entry = GetEntry(criterion);

                if (IsPrimaryKey(criterion))
                    return GetByKeyFromPrimaryDb(entry, entityName);

                return GetFromSecondaryDb(entry, entityName, criterion.Name);
            }

            return GetByJoin(entityName, criteria);
        }


        bool IsPrimaryKey(IDataCriterion criterion) {
            return criterion.Name == "Id"; //todo: something with it
        }


        DatabaseEntry GetEntry(IDataCriterion criterion) {
            if (criterion.Value == null)
                throw new InvalidOperationException("Value of criterion is null");

            return new DatabaseEntry(criterion.Value.ToBinnary());
        }


        #region Implementation of IInternalReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public IEnumerable<object> Read(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(entityName, criteria).Select(x => x.Data.FromBytes());
        }



        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public bool Any(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(entityName, criteria).Any();
        }


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        public IEnumerable<object> ReadColumn(string columnName, string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public int CountOf(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(entityName, criteria).Count();
        }

        #endregion
    }
}