using System;
using System.Collections.Concurrent;
using BerkeleyDB;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.BerkeleyDb {
    class BDbSchema : IBDbSchema {
        readonly string _environmentPath;
        readonly string _dbPath;
        string _password;
        readonly ConcurrentDictionary<string, Database> _primaryReadOnlyDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _secondaryReadOnlyDbs = new ConcurrentDictionary<string, SecondaryDatabase>();

        readonly ConcurrentDictionary<string, Database> _primaryWritableDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _foreignKeyWritableDbs = new ConcurrentDictionary<string, SecondaryDatabase>();


        public bool IsEncrypted { get; private set; }

        public virtual DatabaseEnvironment Environment {
            get {
                if (_environment != null)
                    return _environment;

                _environment = OpenEnvironment(_environmentPath, _dbPath, _password);
                _password = null;

                IsEncrypted = _environment.EncryptAlgorithm == EncryptionAlgorithm.AES;

                return _environment;
            }
        }
        DatabaseEnvironment _environment;

        public BDbSchema(string environmentPath, string dbPath) : this(environmentPath, dbPath, null) { }

        public BDbSchema(string environmentPath, string dbPath, string password) {
            _environmentPath = environmentPath;
            _dbPath = dbPath;
            _password = password;
        }



        public Database GetPrimaryDb(string dbName, bool readOnly) {
            var cache = readOnly ? _primaryReadOnlyDbs : _primaryWritableDbs;

            Database database;
            if (cache.TryGetValue(dbName, out database))
                return database;

            database = OpenDatabase(dbName, readOnly);
            if (cache.TryAdd(dbName, database))
                return database;

            database.Close();
            database = cache[dbName];
            return database;
        }


        public SecondaryDatabase GetReadOnlySecondaryDb(Database primaryDb, string indexSubName, DuplicatesPolicy duplicatesPolicy) {
            var dbName = SecondaryDbName(primaryDb, indexSubName);

            SecondaryDatabase database;
            if (_secondaryReadOnlyDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenReadOnlySecondaryDb(primaryDb, indexSubName, duplicatesPolicy);

            if (_secondaryReadOnlyDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _secondaryReadOnlyDbs[dbName];
            return database;
        }

        public SecondaryDatabase GetWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute foreignKeyMapping) {
            var dbName = ForeignKeyDbName(primaryDb, foreignDb);

            SecondaryDatabase database;

            if (_foreignKeyWritableDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenWritableForeignKeyDatabase(primaryDb, foreignDb, foreignKeyMapping, dbName);

            if (_foreignKeyWritableDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _foreignKeyWritableDbs[dbName];
            return database;
        }





        protected virtual SecondaryDatabase OpenWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute foreignKeyMapping, string dbName) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(primaryDb, GetForeignKeyGenerator(foreignKeyMapping)) {
                Env = Environment,
                Encrypted = IsEncrypted,
                Duplicates = foreignKeyMapping.DuplicatesPolicy,
                Creation = CreatePolicy.IF_NEEDED,
                ReadUncommitted = true,
                AutoCommit = true,
            };

            foreignKeyConfig.SetForeignKeyConstraint(foreignDb, ForeignKeyDeleteAction.ABORT);

            return SecondaryBTreeDatabase.Open(_dbPath, dbName, foreignKeyConfig);
        }


        protected virtual SecondaryDatabase OpenReadOnlySecondaryDb(Database primaryDb, string indexSubName, DuplicatesPolicy duplicatesPolicy) {
            return SecondaryBTreeDatabase.Open(_dbPath,
                SecondaryDbName(primaryDb, indexSubName),
                new SecondaryBTreeDatabaseConfig(primaryDb, (key, data) => null) {
                    Env = Environment,
                    Encrypted = IsEncrypted,
                    Duplicates = duplicatesPolicy,
                    Creation = CreatePolicy.NEVER,
                    ReadOnly = true,
                    AutoCommit = true,
                    ReadUncommitted = true
                });
        }


        protected virtual string SecondaryDbName(Database primaryDb, string indexSubName) {
            return $"{primaryDb.DatabaseName}-->{indexSubName}";
        }


        protected virtual string ForeignKeyDbName(Database primaryDb, Database foreignDb) {
            return $"{primaryDb.DatabaseName}-->{foreignDb.DatabaseName}";
        }


        protected virtual DatabaseEnvironment OpenEnvironment(string environmentPath, string dbPath, string password) {
            var environmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
                Private = true,
                UseLogging = true,
                UseLocking = true,
                FreeThreaded = true,
                UseTxns = true,/*
                LockSystemCfg = new LockingConfig {
                    DeadlockResolution = DeadlockPolicy.MIN_WRITE
                },
                LogSystemCfg = new LogConfig {
                    InMemory = true,
                    BufferSize = 100 * 1024 * 1024
                },
                MPoolSystemCfg = new MPoolConfig {
                    CacheSize = new CacheInfo(0, 100 * 1024 * 1024, 1)
                }*/
            };
            if (!string.IsNullOrWhiteSpace(password)) {
                environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
            }

            return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
        }


        protected virtual Database OpenDatabase(string dbName, bool readOnly) {
            return BTreeDatabase.Open(_dbPath, dbName, new BTreeDatabaseConfig {
                Env = Environment,
                Encrypted = IsEncrypted,
                Creation = CreatePolicy.IF_NEEDED,
                ReadOnly = readOnly,
                AutoCommit = true,
                ReadUncommitted = true
            });
        }


        protected virtual SecondaryKeyGenDelegate GetForeignKeyGenerator(ForeignKeyAttribute foreignKeyMapping) {
            return (pKey, pData) => {
                var entity = pData.Data.FromBytes();

                object foreingKey;
                if (!entity.TryGetValueOnPath(foreignKeyMapping.ForeignKeyPath, out foreingKey))
                    throw new InvalidOperationException();

                return new DatabaseEntry(foreingKey.ToBinnary());
            };
        }


        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Environment.CloseForceSyncAndForceSyncEnv();
            _secondaryReadOnlyDbs.Clear();
            _foreignKeyWritableDbs.Clear();
            _primaryReadOnlyDbs.Clear();
            _primaryWritableDbs.Clear();
        }

        #endregion
    }
}