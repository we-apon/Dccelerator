using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerkeleyDB;
using ConsoleApplication1;
using Dccelerator;
using Dccelerator.DataAccess.BerkeleyDb;
using Dccelerator.DataAccess.Implementations;


namespace ConsoleApp1
{
    public class Program
    {
        static string _logTxt;
        static readonly string _home = AppDomain.CurrentDomain.BaseDirectory;


        public static void Main(string[] args)
        {

            _logTxt = Path.Combine(_home, "log.txt");
            if (File.Exists(_logTxt))
                File.Delete(_logTxt);

#if DEBUG
            var length = 10000;
#else
            var length = 100000;
#endif
            File.AppendAllText(_logTxt, $"Entities count: {length}\nOther entities count: {length * 2}\n\n");

            var entities = new SomeEntity[length];
            var otherEntities = new SomeOtherEntity[length * 2];
            var ids = new byte[length][];
            var serializedEntities = new byte[length][];
            Parallel.For(0,
                length,
                i => {
                    var someEntity = RandomMaker.Make<SomeEntity>(includeGuids: true);

                    var other1 = RandomMaker.Make<SomeOtherEntity>(true, x => x.SomeEntityId = someEntity.Id);
                    var other2 = RandomMaker.Make<SomeOtherEntity>(true, x => x.SomeEntityId = someEntity.Id);

                    otherEntities[i * 2] = other1;
                    otherEntities[i * 2 + 1] = other2;

                    entities[i] = someEntity;
                    serializedEntities[i] = someEntity.Serialize();
                    ids[i] = someEntity.Id.ToByteArray();
                });




            TestBTreehDb(length, ids, entities, otherEntities);

            //TestBTreehDbMt(length, ids, entities, otherEntities);

            var factory = new BDbDataManagerFactory(_home, Path.Combine(_home, "btree.bdb"), "asdasdd");
            var manager = new DataManager(factory);

            var entityId = entities.Shuffle().First().Id;
            var getter = manager.Get<SomeOtherEntity>();

            var twoOtherEntities = getter.Where(x => x.SomeEntityId, entityId).ToList();

        }



        static void TestBTreehDb(int length, byte[][] ids, SomeEntity[] entities, SomeOtherEntity[] otherEntities) {

            GC.Collect();

            var bTreeDbPath = Path.Combine(_home, "btree.bdb");
            if (File.Exists(bTreeDbPath))
                File.Delete(bTreeDbPath);


            var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
                SystemMemory = true,
                Lockdown = true,
                ErrorPrefix = "Environment: ",
                ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
            };
            databaseEnvironmentConfig.SetEncryption("asdasdd", EncryptionAlgorithm.AES);
            var env = DatabaseEnvironment.Open(_home, databaseEnvironmentConfig);


            var otherEntitiesDb = OpenBTreeDb(bTreeDbPath, nameof(SomeOtherEntity), env); //primary
            var entitiesDb = OpenBTreeDb(bTreeDbPath, nameof(SomeEntity), env); // foreign

            var foreignKey = MakeForeingKey(bTreeDbPath, otherEntitiesDb, entitiesDb, env); // foreign key and constraint



            var watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var data = new DatabaseEntry(entities[i].ToBinnary());
                entitiesDb.Put(id, data);

                var other1 = otherEntities[i*2];
                var other1Id = new DatabaseEntry(other1.Id.ToByteArray());
                var other1Data = new DatabaseEntry(other1.ToBinnary());
                otherEntitiesDb.Put(other1Id, other1Data);

                var other2 = otherEntities[i*2 +1];
                var other2Id = new DatabaseEntry(other2.Id.ToByteArray());
                var other2Data = new DatabaseEntry(other2.ToBinnary());
                otherEntitiesDb.Put(other2Id, other2Data);

            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Put in b-tree: " + watch.Elapsed + "\n");

            entitiesDb.Sync();
            otherEntitiesDb.Sync();
            foreignKey.Sync();


/*
            watch.Restart();
            for (int i = 0; i < length*2; i++) {
                var otherEntity = otherEntities[i];
                var id = new DatabaseEntry(otherEntity.Id.ToByteArray());

                var text = otherEntity.ToJson();
                var data = new DatabaseEntry(Encoding.UTF8.GetBytes(text));

                otherEntitiesDb.Put(id, data);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Put in b-tree: " + watch.Elapsed + "\n");
*/






            var resultPairs = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();

            watch.Restart();
            using (var cursor = entitiesDb.Cursor()) {
                while (cursor.MoveNext()) {
                    var current = cursor.Current;
                    resultPairs.Add(current);
                }
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous read from entities b-tree: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();

            watch.Restart();
            using (var cursor = otherEntitiesDb.Cursor()) {
                while (cursor.MoveNext()) {
                    var current = cursor.Current;
                    resultPairs.Add(current);
                }
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous read from other entities b-tree: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();



            watch.Restart();
            foreach (var idBytes in ids.Shuffle()) {
                var id = new DatabaseEntry(idBytes);
                var result = entitiesDb.Get(id);
                resultPairs.Add(result);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in entities b-tree: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();

            watch.Restart();
            foreach (var otherEntity in otherEntities.Shuffle()) {
                var id = new DatabaseEntry(otherEntity.Id.ToByteArray());
                var result = otherEntitiesDb.Get(id);
                resultPairs.Add(result);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in other entities b-tree: " + watch.Elapsed + "\n");


            resultPairs.Clear();
            GC.Collect();

            watch.Restart();
            var foreignCursor = foreignKey.Cursor();
            foreach (var idBytes in ids.Shuffle()) {
                var someEntityId = new DatabaseEntry(idBytes);

                if (!foreignCursor.Move(someEntityId, exact: true))
                    continue;

                resultPairs.Add(foreignCursor.Current);

                while (foreignCursor.MoveNextDuplicate()) {
                    resultPairs.Add(foreignCursor.Current);
                }
            }

            foreignCursor.Close();
            watch.Stop();
            File.AppendAllText(_logTxt, "Search other entity by id of entity (by foreign key) in b-tree: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();
            
            

            /*
                        var concurrentResults = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
                        watch.Restart();
                        Parallel.ForEach(ids,
                            bytes => {
                                var id = new DatabaseEntry(bytes);
                                var result = bTreeDb.Get(id);
                                concurrentResults.Add(result);
                            });
                        watch.Stop();
                        File.AppendAllText(_logTxt, "Parallel search by key in b-tree: " + watch.Elapsed + "\n");
                        GC.Collect();
            */


            watch.Restart();
            entitiesDb.Sync();
            otherEntitiesDb.Sync();
            foreignKey.Sync();
            File.AppendAllText(_logTxt, "b-tree sync: " + watch.Elapsed + "\n\n");

/*            foreach (var index in indexes) {
                index.Close();
            }*/
            
            foreignKey.Close();
            otherEntitiesDb.Close();
            otherEntitiesDb.Dispose();
            entitiesDb.Close();
            entitiesDb.Dispose();
            GC.Collect();
        }



        static SecondaryBTreeDatabase MakeForeingKey(string bTreeDbPath, BTreeDatabase otherEntitiesDb, BTreeDatabase entitiesDb, DatabaseEnvironment env) {
            var foreignKeyConfig = new SecondaryBTreeDatabaseConfig(
                otherEntitiesDb,
                (pKey, pData) => {
                    var otherEntity = pData.Data.FromBytes<SomeOtherEntity>();

                    var secondaryId = otherEntity.SomeEntityId.ToByteArray();
                    return new DatabaseEntry(secondaryId);
                }) {
                    Env = env,
                    Encrypted = env?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                    Duplicates = DuplicatesPolicy.SORTED,
                    Creation = CreatePolicy.IF_NEEDED,
                    ErrorPrefix = $"{otherEntitiesDb.DatabaseName}.{entitiesDb.DatabaseName} Db:",
                    ErrorFeedback = (prefix, message) => 
                    File.AppendAllText(_logTxt, prefix + message + "\n")
                };

            foreignKeyConfig.SetForeignKeyConstraint(entitiesDb, ForeignKeyDeleteAction.ABORT);


            var secondary = SecondaryBTreeDatabase.Open(bTreeDbPath, $"{otherEntitiesDb.DatabaseName}.{entitiesDb.DatabaseName}", foreignKeyConfig);
            return secondary;
        }


        static BTreeDatabase OpenBTreeDb(string filePath, string dbName, DatabaseEnvironment environment) {
            var db = BTreeDatabase.Open(filePath, dbName,
                new BTreeDatabaseConfig {
                    Env = environment,
                    Encrypted = environment?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                    Creation = CreatePolicy.IF_NEEDED,
                    ReadOnly = false,
                    ErrorPrefix = $"{dbName}Db: ",
                    ErrorFeedback = (prefix, message) => 
                    File.AppendAllText(_logTxt, prefix + message + "\n"),
                });
            return db;
        }

        


    }
}
