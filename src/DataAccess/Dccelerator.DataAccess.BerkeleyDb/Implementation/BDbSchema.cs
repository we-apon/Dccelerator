using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BerkeleyDB;
using Dccelerator.Reflection;
using Dccelerator.DataAccess.Infrastructure;

namespace Dccelerator.DataAccess.BerkeleyDb.Implementation {
    public class BDbSchema : IBDbSchema {
        readonly string _environmentPath;
        readonly string _dbPath;
        string _password;
        //readonly ConcurrentDictionary<string, Database> _primaryReadOnlyDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _secondaryReadOnlyDbs = new ConcurrentDictionary<string, SecondaryDatabase>();

        readonly ConcurrentDictionary<string, Database> _primaryWritableDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _foreignKeyWritableDbs = new ConcurrentDictionary<string, SecondaryDatabase>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _secondaryKeyWritableDbs = new ConcurrentDictionary<string, SecondaryDatabase>();

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


        /// <exception cref="InvalidOperationException">Can't create environment directory.</exception>
        /// <exception cref="ArgumentException">BerkeleyDb file name is an absolute or relative path, but should be just file name (with or without extension).</exception>
        public BDbSchema(string environmentPath, string dbPath, string password) {
            _environmentPath = environmentPath;
            _dbPath = dbPath;
            _password = password;

            if (Path.GetFileName(_dbPath) != _dbPath) {
                var msg = $"BerkeleyDb file name '{dbPath}' is an absolute or relative path, but should be just file name (with or without extension)";
                Internal.TraceEvent(TraceEventType.Error, msg);
                throw new ArgumentException(msg, nameof(dbPath));
            }

            if (!Directory.Exists(_environmentPath)) {
                try {
                    Directory.CreateDirectory(_environmentPath);
                }
                catch (Exception e) {
                    var msg = $"Can't create environment directory {environmentPath}";
                    Internal.TraceEvent(TraceEventType.Error, msg);
                    throw new InvalidOperationException(msg, e);
                }
            }
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


        public SecondaryDatabase GetReadOnlySecondaryDb(Database primaryDb, SecondaryKeyAttribute secondaryKey) {
            var dbName = SecondaryKeyDbName(primaryDb, secondaryKey);

            SecondaryDatabase database;
            if (_secondaryReadOnlyDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenReadOnlySecondaryDb(primaryDb, secondaryKey);

            if (_secondaryReadOnlyDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _secondaryReadOnlyDbs[dbName];
            return database;
        }

        public SecondaryDatabase GetWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, SecondaryKeyAttribute foreignKeyMapping) {
            var dbName = SecondaryKeyDbName(primaryDb, foreignKeyMapping);

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


        public SecondaryDatabase GetWritableSecondaryKeyDatabase(Database primaryDb, SecondaryKeyAttribute secondaryKey) {
            var dbName = SecondaryKeyDbName(primaryDb, secondaryKey);

            SecondaryDatabase database;

            if (_secondaryKeyWritableDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenWritableSecondaryDatabase(primaryDb, secondaryKey, dbName);

            if (_secondaryKeyWritableDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _secondaryKeyWritableDbs[dbName];
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

                foreach (var keyMapping in info.SecondaryKeys.Where(x => !info.ForeignKeys.ContainsKey(x.Key))) {
                    GetWritableSecondaryKeyDatabase(primaryDb, keyMapping.Value);
                }

                lock (_transactionPreparedEntities) {
                    _transactionPreparedEntities.Add(info.EntityName);
                }
            }

            return Environment.BeginTransaction();
        }


        protected virtual SecondaryDatabase OpenWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, SecondaryKeyAttribute secondaryForeingKey, string dbName) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(primaryDb, GetForeignKeyGenerator(secondaryForeingKey)) {
                Env = Environment,
                Encrypted = IsEncrypted,
                Duplicates = (BerkeleyDB.DuplicatesPolicy)secondaryForeingKey.DuplicatesPolicy,
                Creation = CreatePolicy.IF_NEEDED,
                ReadUncommitted = true,
                FreeThreaded = IsFreeThreaded,
                AutoCommit = true,
            };

            foreignKeyConfig.SetForeignKeyConstraint(foreignDb, ForeignKeyDeleteAction.ABORT);

            return SecondaryBTreeDatabase.Open(_dbPath, dbName, foreignKeyConfig);
        }
        protected virtual SecondaryDatabase OpenWritableSecondaryDatabase(Database primaryDb, SecondaryKeyAttribute secondaryForeingKey, string dbName) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(primaryDb, GetForeignKeyGenerator(secondaryForeingKey)) {
                Env = Environment,
                Encrypted = IsEncrypted,
                Duplicates = (BerkeleyDB.DuplicatesPolicy)secondaryForeingKey.DuplicatesPolicy,
                Creation = CreatePolicy.IF_NEEDED,
                ReadUncommitted = true,
                FreeThreaded = IsFreeThreaded,
                AutoCommit = true,
            };

            return SecondaryBTreeDatabase.Open(_dbPath, dbName, foreignKeyConfig);
        }


        protected virtual SecondaryDatabase OpenReadOnlySecondaryDb(Database primaryDb, SecondaryKeyAttribute secondaryKey) {
            return SecondaryBTreeDatabase.Open(_dbPath, //bug: when open fails - it means that key is new, so we should open new writtable db and make indexes for existed values
                SecondaryKeyDbName(primaryDb, secondaryKey),
                new SecondaryBTreeDatabaseConfig(primaryDb, (key, data) => null) {
                    Env = Environment,
                    Encrypted = IsEncrypted,
                    Duplicates = (BerkeleyDB.DuplicatesPolicy)secondaryKey.DuplicatesPolicy,
                    Creation = CreatePolicy.NEVER,
                    ReadOnly = true,
                    AutoCommit = true,
                    FreeThreaded = IsFreeThreaded,
                    ReadUncommitted = true
                });
        }


/*        protected virtual string SecondaryDbName(Database primaryDb, string indexSubName) {
            return $"{primaryDb.DatabaseName}-->{indexSubName}";
        }*/

/*        [Obsolete]
        protected virtual string ForeignKeyDbName(Database primaryDb, Database foreignDb) {
            return $"{primaryDb.DatabaseName}-->{foreignDb.DatabaseName}";
        }*/

        protected virtual string SecondaryKeyDbName(Database primaryDb, SecondaryKeyAttribute secondaryKey) {
            return $"{primaryDb.DatabaseName}-->{secondaryKey.Name}";
        }


        protected virtual DatabaseEnvironment OpenEnvironment(string environmentPath, string dbPath, string password) {
            try {
                var environmentConfig = GetDefaultEnvironmentConfig();
                if (!string.IsNullOrWhiteSpace(password)) {
                    environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
                }
                return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
            }
            catch (RunRecoveryException e) {
                try {
                    var environmentConfig = GetDefaultEnvironmentConfig();
                    environmentConfig.RunRecovery = true;
                    if (!string.IsNullOrWhiteSpace(password)) {
                        environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
                    }
                    return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
                }
                catch (RunRecoveryException exception) {
                    try {
                        var environmentConfig = GetDefaultEnvironmentConfig();
                        environmentConfig.RunFatalRecovery = true;
                        if (!string.IsNullOrWhiteSpace(password)) {
                            environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
                        }
                        return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
                    }
                    catch (Exception ex) {
                        throw new InvalidOperationException($"Can't open environment even with recovery!", ex);
                    }
                }
            }
        }


        protected virtual DatabaseEnvironmentConfig GetDefaultEnvironmentConfig() {
            var environmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
                Private = false,
                UseLogging = true,
                UseLocking = true,
                FreeThreaded = true,
                UseTxns = true,
                LockSystemCfg = GetLockSystemConfig(),
                LogSystemCfg = GetLogSystemConfig(),
                MPoolSystemCfg = GetMPoolSystemConfig()
            };
            return environmentConfig;
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


        protected virtual SecondaryKeyGenDelegate GetForeignKeyGenerator(SecondaryKeyAttribute secondaryKey) {
            return (pKey, pData) => {
                var entity = pData.Data.FromBytes();

                object foreingKey;
                if (!entity.TryGetValueOnPath(secondaryKey.Name, out foreingKey))
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