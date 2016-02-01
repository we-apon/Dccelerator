using System;
using BerkeleyDB;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public abstract class BTreeBDbRepository : BdbRepositoryBase {
        readonly string _environmentPath;
        readonly string _dbPath;
        readonly string _password;
        readonly EncryptionAlgorithm _encryptionAlgorithm;


        protected BTreeBDbRepository(string environmentPath, string dbPath, string password, EncryptionAlgorithm encryptionAlgorithm) { 
            _environmentPath = environmentPath;
            _dbPath = dbPath;
            _password = password;
            _encryptionAlgorithm = encryptionAlgorithm;
        }


        protected override DatabaseEnvironment OpenEnvironment() {
            var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
                Private = true,
            };

            if (_encryptionAlgorithm != EncryptionAlgorithm.DEFAULT)
                databaseEnvironmentConfig.SetEncryption(_password, EncryptionAlgorithm.AES);

            return DatabaseEnvironment.Open(_environmentPath, databaseEnvironmentConfig);
        }



        protected override Database OpenReadOnlyPrimaryDb(string dbName, DatabaseEnvironment environment) {
            return BTreeDatabase.Open(_dbPath, dbName, new BTreeDatabaseConfig {
                Env = environment,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Creation = CreatePolicy.NEVER,
                ReadOnly = true,
            });
        }


        protected override Database OpenPrimaryDb(string dbName, DatabaseEnvironment environment) {
            return BTreeDatabase.Open(_dbPath, dbName, new BTreeDatabaseConfig {
                Env = environment,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Creation = CreatePolicy.IF_NEEDED,
                ReadOnly = false,
            });
        }


        protected override SecondaryDatabase OpenReadOnlySecondaryDb(Database primaryDb, string dbName, DatabaseEnvironment environment) {
            return SecondaryBTreeDatabase.Open(_dbPath, $"{primaryDb.DatabaseName}.{dbName}", new SecondaryBTreeDatabaseConfig(primaryDb, (key, data) => null) {
                Env = environment,
                ReadOnly = true,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Duplicates = DuplicatesPolicy.UNSORTED,
                Creation = CreatePolicy.NEVER
            });
        }


        protected override SecondaryDatabase OpenForeignKeyDatabase(Database primaryDb, Database foreignDb, BDbMapping mapping, DatabaseEnvironment environment) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(
                primaryDb,
                GetForeignKeyGenerator(mapping)) {
                    Env = environment,
                    Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                    Duplicates = DuplicatesPolicy.UNSORTED,
                    Creation = CreatePolicy.IF_NEEDED
                };

            foreignKeyConfig.SetForeignKeyConstraint(foreignDb, ForeignKeyDeleteAction.ABORT);

            var secondary = SecondaryBTreeDatabase.Open(_dbPath, $"{primaryDb.DatabaseName}.{foreignDb.DatabaseName}", foreignKeyConfig);
            return secondary;
        }

        


        public virtual SecondaryKeyGenDelegate GetForeignKeyGenerator(BDbMapping mapping) {
            return (pKey, pData) => {
                var entity = pData.Data.FromBytes();

                object foreingKey;
                if (!entity.TryGetValueOnPath(mapping.ForeignKeyPath, out foreingKey))
                    throw new InvalidOperationException();

                return new DatabaseEntry(foreingKey.ToBinnary());
            };
        }


        protected override DatabaseEntry DataOf(object entity) {
            if (entity == null)
                return null;

            return new DatabaseEntry(entity.ToBinnary());
        }
    }
}