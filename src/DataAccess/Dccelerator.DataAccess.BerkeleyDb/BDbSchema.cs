using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BerkeleyDB;
using Dccelerator.DataAccess.Attributes;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.BerkeleyDb {
    class BDbSchema : IBDbSchema {
        readonly string _environmentPath;
        readonly string _dbPath;
        string _password;
        //readonly ConcurrentDictionary<string, Database> _primaryReadOnlyDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _secondaryReadOnlyDbs = new ConcurrentDictionary<string, SecondaryDatabase>();

        readonly ConcurrentDictionary<string, Database> _primaryWritableDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _foreignKeyWritableDbs = new ConcurrentDictionary<string, SecondaryDatabase>();

        readonly HashSet<string> _transactionPreparedEntities = new HashSet<string>(); 



        public bool IsEncrypted { get; private set; }

        public bool IsFreeThreaded { get; private set; }

        public virtual DatabaseEnvironment Environment {
            get {
                if (_environment != null)
                    return _environment;

                _environment = OpenEnvironment(_environmentPath, _dbPath, _password);
                _password = null;

                IsEncrypted = _environment.EncryptAlgorithm == EncryptionAlgorithm.AES;
                IsFreeThreaded = _environment.FreeThreaded;

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
            Database database;
            if (_primaryWritableDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenDatabase(dbName);
            if (_primaryWritableDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _primaryWritableDbs[dbName];
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


        public virtual Transaction BeginTransactionFor(ICollection<IBDbEntityInfo> entityInfos) {
            var needToPrepare = new List<IBDbEntityInfo>();

            lock (_transactionPreparedEntities) {
                needToPrepare.AddRange(entityInfos.Where(info => !_transactionPreparedEntities.Contains(info.EntityName)));
            }

            foreach (var info in needToPrepare) {
                var primaryDb = GetPrimaryDb(info.EntityName, readOnly:false);

                foreach (var keyMapping in info.ForeignKeys.Values) {
                    var foreignDb = GetPrimaryDb(keyMapping.ForeignEntityName, readOnly:false);
                    GetWritableForeignKeyDatabase(primaryDb, foreignDb, keyMapping);
                }

                lock (_transactionPreparedEntities) {
                    _transactionPreparedEntities.Add(info.EntityName);
                }
            }

            return Environment.BeginTransaction();
        }


        protected virtual SecondaryDatabase OpenWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute foreignKeyMapping, string dbName) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(primaryDb, GetForeignKeyGenerator(foreignKeyMapping)) {
                Env = Environment,
                Encrypted = IsEncrypted,
                Duplicates = (BerkeleyDB.DuplicatesPolicy)foreignKeyMapping.DuplicatesPolicy,
                Creation = CreatePolicy.IF_NEEDED,
                ReadUncommitted = true,
                FreeThreaded = IsFreeThreaded,
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
                    Duplicates = (BerkeleyDB.DuplicatesPolicy)duplicatesPolicy,
                    Creation = CreatePolicy.NEVER,
                    ReadOnly = true,
                    AutoCommit = true,
                    FreeThreaded = IsFreeThreaded,
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
                UseTxns = true,
                LockSystemCfg = GetLockSystemConfig(),
                LogSystemCfg = GetLogSystemConfig(),
                MPoolSystemCfg = GetMPoolSystemConfig()
            };
            if (!string.IsNullOrWhiteSpace(password)) {
                environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
            }

            return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
        }


        protected virtual LockingConfig GetLockSystemConfig() {
            return new LockingConfig {
                DeadlockResolution = DeadlockPolicy.MIN_WRITE,
            };
        }


        protected virtual MPoolConfig GetMPoolSystemConfig() {
            return new MPoolConfig {
                CacheSize = new CacheInfo(0, 500 * 1024 * 1024, 1)
            };
        }


        protected virtual LogConfig GetLogSystemConfig() {
            return new LogConfig {
                InMemory = false,
                BufferSize = 500 * 1024 * 1024
            };
        }


        protected virtual Database OpenDatabase(string dbName) {
            return BTreeDatabase.Open(_dbPath, dbName, new BTreeDatabaseConfig {
                Env = Environment,
                Encrypted = IsEncrypted,
                Creation = CreatePolicy.IF_NEEDED,
                FreeThreaded = IsFreeThreaded,
                AutoCommit = true,
                ReadUncommitted = true
            });
        }


        protected virtual SecondaryKeyGenDelegate GetForeignKeyGenerator(ForeignKeyAttribute foreignKeyMapping) {
            return (pKey, pData) => {
                var entity = pData.Data.FromBytes();

                object foreingKey;
                if (!entity.TryGetValueOnPath(foreignKeyMapping.Name, out foreingKey))
                    throw new InvalidOperationException();

                return new DatabaseEntry(foreingKey.ToBinnary());
            };
        }


        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            foreach (var db in _secondaryReadOnlyDbs) {
                db.Value.Close(true);
                db.Value.Dispose();
            }

            foreach (var db in _foreignKeyWritableDbs) {
                db.Value.Close(true);
                db.Value.Dispose();
            }

            foreach (var db in _primaryWritableDbs) {
                db.Value.Close(true);
                db.Value.Dispose();
            }



            Environment.Close();
            _secondaryReadOnlyDbs.Clear();
            _foreignKeyWritableDbs.Clear();
            /*_primaryReadOnlyDbs.Clear();*/
            _primaryWritableDbs.Clear();
        }

        #endregion
    }
}