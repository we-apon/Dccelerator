using System;
using BerkeleyDB;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public abstract class BTreeBDbRepository : BDbRepositoryBase {
        readonly string _dbPath;


        protected BTreeBDbRepository(string environmentPath, string dbPath, string password, EncryptionAlgorithm encryptionAlgorithm)
            : base(environmentPath, password, encryptionAlgorithm) {
            _dbPath = dbPath;
        }



        protected override Database OpenReadOnlyPrimaryDb(string dbName, DatabaseEnvironment environment) {
            return BTreeDatabase.Open(_dbPath, dbName, new BTreeDatabaseConfig {
                Env = environment,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Creation = CreatePolicy.NEVER,
                ReadOnly = true,
                AutoCommit = true,
                ReadUncommitted = true
            });
        }


        protected override Database OpenPrimaryDb(string dbName, DatabaseEnvironment environment) {
            return BTreeDatabase.Open(_dbPath, dbName, new BTreeDatabaseConfig {
                Env = environment,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Creation = CreatePolicy.IF_NEEDED,
                ReadOnly = false,
                AutoCommit = true,
                ReadUncommitted = true
            });
        }


        protected override SecondaryDatabase OpenReadOnlySecondaryDb(Database primaryDb, string indexSubName, DatabaseEnvironment environment, DuplicatesPolicy duplicatesPolicy) {
            return SecondaryBTreeDatabase.Open(_dbPath, SecondaryDbName(primaryDb, indexSubName), new SecondaryBTreeDatabaseConfig(primaryDb, (key, data) => null) {
                Env = environment,
                ReadOnly = true,
                Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                Duplicates = duplicatesPolicy,
                Creation = CreatePolicy.NEVER,
                AutoCommit = true,
                ReadUncommitted = true
            });
        }




        protected override SecondaryDatabase OpenForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute foreignKeyMapping, DatabaseEnvironment environment) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(
                primaryDb,
                GetForeignKeyGenerator(foreignKeyMapping)) {
                    Env = environment,
                    Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                    Duplicates = foreignKeyMapping.DuplicatesPolicy,
                    Creation = CreatePolicy.IF_NEEDED,
                    ReadUncommitted = true,
                    AutoCommit = true,
                };

            foreignKeyConfig.SetForeignKeyConstraint(foreignDb, ForeignKeyDeleteAction.ABORT);

            var secondary = SecondaryBTreeDatabase.Open(_dbPath, ForeignKeyDbName(primaryDb, foreignDb), foreignKeyConfig);
            return secondary;
        }




        public virtual SecondaryKeyGenDelegate GetForeignKeyGenerator(ForeignKeyAttribute foreignKeyMapping) {
            return (pKey, pData) => {
                var entity = pData.Data.FromBytes();

                object foreingKey;
                if (!entity.TryGetValueOnPath(foreignKeyMapping.ForeignKeyPath, out foreingKey))
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