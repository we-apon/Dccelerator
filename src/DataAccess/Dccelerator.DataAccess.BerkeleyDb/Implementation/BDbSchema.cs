using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BerkeleyDB;
using Dccelerator.Reflection;
using Dccelerator.DataAccess.Infrastructure;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.BerkeleyDb.Implementation {
    public class BDbSchema : IBDbSchema {
        readonly string _environmentPath;
        readonly string _dbPath;
        string _password;

        readonly ConcurrentDictionary<string, Database> _primaryDbs = new ConcurrentDictionary<string, Database>();
        readonly ConcurrentDictionary<string, SecondaryDatabase> _secondaryDbs = new ConcurrentDictionary<string, SecondaryDatabase>();
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



        public virtual Database GetPrimaryDb(string dbName) {
            Database database;
            if (_primaryDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenDatabase(dbName);
            if (_primaryDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _primaryDbs[dbName];
            return database;
        }
        

        public virtual SecondaryDatabase GetSecondaryDb(Database primaryDb, SecondaryKeyAttribute secondaryKey, Database foreignDb = null) {
            var dbName = SecondaryKeyDbName(primaryDb, secondaryKey);

            SecondaryDatabase database;

            if (_secondaryDbs.TryGetValue(dbName, out database))
                return database;

            database = OpenWritableSecondaryDatabase(primaryDb, secondaryKey, dbName, foreignDb);

            if (_secondaryDbs.TryAdd(dbName, database))
                return database;

            database.Close();
            database = _secondaryDbs[dbName];
            return database;
        }

        
        

        

        public virtual Transaction BeginTransactionFor(ICollection<IBDbEntityInfo> entityInfos) {
            var needToPrepare = new List<IBDbEntityInfo>();

            lock (_transactionPreparedEntities) {
                needToPrepare.AddRange(entityInfos.Where(info => !_transactionPreparedEntities.Contains(info.EntityName)));
            }

            foreach (var info in needToPrepare) {
                var primaryDb = GetPrimaryDb(info.EntityName);

                foreach (var keyMapping in info.ForeignKeys.Values) {
                    var foreignDb = GetPrimaryDb(keyMapping.ForeignEntityName);
                    GetSecondaryDb(primaryDb, keyMapping, foreignDb);
                }

                foreach (var keyMapping in info.SecondaryKeys.Where(x => !info.ForeignKeys.ContainsKey(x.Key)).Select(x => x.Value)) {
                    GetSecondaryDb(primaryDb, keyMapping);
                }

                lock (_transactionPreparedEntities) {
                    _transactionPreparedEntities.Add(info.EntityName);
                }
            }

            return Environment.BeginTransaction();
        }

        
        protected virtual SecondaryDatabase OpenWritableSecondaryDatabase(Database primaryDb,SecondaryKeyAttribute secondaryForeingKey, string dbName, [CanBeNull] Database foreignDb = null) {
            var config = new SecondaryBTreeDatabaseConfig(primaryDb, GetForeignKeyGenerator(secondaryForeingKey)) {
                Env = Environment,
                Encrypted = IsEncrypted,
                Duplicates = (BerkeleyDB.DuplicatesPolicy)secondaryForeingKey.DuplicatesPolicy,
                Creation = CreatePolicy.IF_NEEDED,
                ReadUncommitted = true,
                FreeThreaded = IsFreeThreaded,
                AutoCommit = true,
            };

            if (foreignDb != null)
                config.SetForeignKeyConstraint(foreignDb, ForeignKeyDeleteAction.ABORT);

            return SecondaryBTreeDatabase.Open(_dbPath, dbName, config);
        }
        

        protected virtual string SecondaryKeyDbName(Database primaryDb, SecondaryKeyAttribute secondaryKey) {
            return $"{primaryDb.DatabaseName}-->{secondaryKey.Name}";
        }


        protected virtual DatabaseEnvironment OpenEnvironment(string environmentPath, string dbPath, string password) {
            try {
                var environmentConfig = GetDefaultEnvironmentConfig();
                if (!string.IsNullOrWhiteSpace(password)) {
                    environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
                }
                var environment = DatabaseEnvironment.Open(_environmentPath, environmentConfig);

/*
                Task.Factory.StartNew(() => {
                    while (true) {
                        environment.DetectDeadlocks(DeadlockPolicy.MIN_WRITE);
                        Thread.Sleep(10);
                    }
                }, TaskCreationOptions.LongRunning);
*/

                return environment;
            }
            catch (RunRecoveryException) {
                try {
                    var environmentConfig = GetDefaultEnvironmentConfig();
                    environmentConfig.RunRecovery = true;
                    if (!string.IsNullOrWhiteSpace(password)) {
                        environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
                    }
                    return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
                }
                catch (RunRecoveryException) {
                    try {
                        var environmentConfig = GetDefaultEnvironmentConfig();
                        environmentConfig.RunFatalRecovery = true;
                        if (!string.IsNullOrWhiteSpace(password)) {
                            environmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);
                        }
                        return DatabaseEnvironment.Open(_environmentPath, environmentConfig);
                    }
                    catch (Exception ex) {
                        throw new InvalidOperationException("Can't open environment even with recovery!", ex);
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
                ForceFlush = true,
                LockSystemCfg = GetLockingConfig(),
                LogSystemCfg = GetLogsConfig(),
                MPoolSystemCfg = GetMPoolConfig(),
                MutexSystemCfg = GetMutexesConfig(),
            };

            return environmentConfig;
        }


        protected virtual MutexConfig GetMutexesConfig() {
            return new MutexConfig {
                InitMutexes = 0,
                MaxMutexes = 0,
                Increment = 0
            };
        }


        protected virtual LockingConfig GetLockingConfig() {
            return new LockingConfig {
                DeadlockResolution = DeadlockPolicy.MIN_WRITE,
            };
        }


        protected virtual MPoolConfig GetMPoolConfig() {
            return new MPoolConfig {
                CacheSize = new CacheInfo(0, 500 * 1024 * 1024, 2)
            };
        }

        protected virtual LogConfig GetLogsConfig() {
            return new LogConfig {
                InMemory = false,
                BufferSize = 50 * 1024 * 1024
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
                if (!entity.TryGet(secondaryKey.Name, out foreingKey))
                    throw new InvalidOperationException();

                return new DatabaseEntry(foreingKey.ToBinnary());
            };
        }


        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {

            foreach (var db in _secondaryDbs) {
                db.Value.Close(true);
                db.Value.Dispose();
            }

            foreach (var db in _primaryDbs) {
                db.Value.Close(true);
                db.Value.Dispose();
            }

            Environment.Close();
        }

        #endregion
    }
}