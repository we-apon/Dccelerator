using System;
using System.Collections.Generic;
using System.IO;
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
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Duplicates = DuplicatesPolicy.UNSORTED,
                Creation = CreatePolicy.NEVER
            });
        }


        #region Implementation of IInternalReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public IEnumerable<object> Read(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            DatabaseEnvironment environment = null;

            environment = GetEnvironment();

            using (var primaryDb = OpenReadOnlyPrimaryDb(_dbFilePath, entityName, environment)) {

                if (criteria.Count == 0) {
                    using (var cursor = primaryDb.Cursor()) {
                        while (cursor.MoveNext()) {
                            yield return cursor.Current.Value.Data.FromBytes(entityType);
                        }
                        cursor.Close();
                    }

                    primaryDb.Close();
                    environment.Close();
                    yield break;
                }


                if (criteria.Count == 1) {
                    var criterion = criteria.First();
                    var entry = GetEntry(criterion);

                    if (IsKey(criterion)) {
                        if (primaryDb.Exists(entry))
                            yield return primaryDb.Get(entry).Value.Data.FromBytes(entityType);

                        primaryDb.Close();
                        environment.Close();
                        yield break;
                    }

                    var secondary = OpenReadOnlySecondaryDb(primaryDb, _dbFilePath, criterion.Name, environment);
                    if (secondary.Duplicates == DuplicatesPolicy.NONE) {
                        if (secondary.Exists(entry))
                            yield return secondary.Get(entry).Value.Data.FromBytes(entityType);

                        secondary.Close();
                        primaryDb.Close();
                        environment.Close();
                        yield break;
                    }

                    var cursor = secondary.Cursor();
                    if (!cursor.Move(entry, exact: true)) {
                        cursor.Close();
                        secondary.Close();
                        primaryDb.Close();
                        environment.Close();
                        yield break;
                    }

                    yield return cursor.Current.Value.Data.FromBytes(entityType);
                    while (cursor.MoveNextDuplicate()) {
                        yield return cursor.Current.Value.Data.FromBytes(entityType);
                    }

                    cursor.Close();
                    secondary.Close();
                    primaryDb.Close();
                    environment.Close();
                    yield break;
                }



                var length = 0;

                var secondaryDbs = new SecondaryBTreeDatabase[criteria.Count];
                var secondaryCursors = new SecondaryCursor[criteria.Count];

                foreach (var criterion in criteria) {
                    var secondaryDb = OpenReadOnlySecondaryDb(primaryDb, _dbFilePath, criterion.Name, environment);
                    var cursor = secondaryDb.SecondaryCursor();

                    var key = new DatabaseEntry(criterion.Value.ToBinnary());
                    if (!cursor.Move(key, true)) {
                        yield break;
                    }

                    secondaryDbs[length] = secondaryDb;
                    secondaryCursors[length++] = cursor;
                }


                var joinCursor = primaryDb.Join(secondaryCursors, true);
                while (joinCursor.MoveNext()) {
                    var current = joinCursor.Current;
                    yield return current.Value.Data.FromBytes(entityType);
                }

                joinCursor.Close();
                for (int i = 0; i < length; i++) {
                    secondaryCursors[i].Close();
                    secondaryDbs[i].Close();
                }
                primaryDb.Close();
                environment.Close();
            }

        }


        DatabaseEnvironment GetEnvironment() {
            Console.WriteLine("Envir path: " + _environmentPath);

            var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
                SystemMemory = true,
                Lockdown = true
            };
            databaseEnvironmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
            var environment = DatabaseEnvironment.Open(_environmentPath, databaseEnvironmentConfig);
            return environment;
        }


        bool IsKey(IDataCriterion criterion) {
            return criterion.Name == "Id"; //todo: something with it
        }


        DatabaseEntry GetEntry(IDataCriterion criterion) {
            if (criterion.Value == null)
                throw new InvalidOperationException("Value of criterion is null");

            return new DatabaseEntry(criterion.Value.ToBinnary());
        }


        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public bool Any(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        #endregion
    }
}