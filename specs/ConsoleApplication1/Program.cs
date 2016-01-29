using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerkeleyDB;
using Dccelerator;
using Dccelerator.Reflection;
using ServiceStack;


namespace ConsoleApplication1
{
    class Program
    {
        static readonly string _home = AppDomain.CurrentDomain.BaseDirectory;
        static string _logTxt;


        static void Main(string[] args) {

            _logTxt = Path.Combine(_home, "log.txt");
            if (File.Exists(_logTxt))
                File.Delete(_logTxt);

#if DEBUG
            var length = 10000;
#else
            var length = 100000;
#endif
            File.AppendAllText(_logTxt, $"Entities count: {length}\nOther entities count: {length*2}\n\n");

            var entities = new SomeEntity[length];
            var otherEntities = new SomeOtherEntity[length*2];
            var ids = new byte[length][];
            var serializedEntities = new byte[length][];
            Parallel.For(0,
                length,
                i => {
                    var someEntity = RandomMaker.Make<SomeEntity>(includeGuids: true);

                    var other1 = RandomMaker.Make<SomeOtherEntity>(true, x => x.SomeEntityId = someEntity.Id);
                    var other2 = RandomMaker.Make<SomeOtherEntity>(true, x => x.SomeEntityId = someEntity.Id);

                    otherEntities[i*2] = other1;
                    otherEntities[i*2 + 1] = other2;

                    entities[i] = someEntity;
                    serializedEntities[i] = someEntity.Serialize();
                    ids[i] = someEntity.Id.ToByteArray();
                });




            TestBTreehDb(length, ids, entities, otherEntities);

            TestBTreehDbMt(length, ids, entities, otherEntities);

/*

            TestHashDbEncrypted(length, ids, serializedEntities);

            TestBTreehDbEncrypted(length, ids, serializedEntities);
*/

/*            TestHashDb(length, ids, serializedEntities);
            


            TestHashLoadFromFile();

            TestBTreeLoadFromFile();*/
        }



        static void TestHashLoadFromFile() {
            GC.Collect();

            var hashDbPath = Path.Combine(_home, "hash.bdb");

            var watch = new Stopwatch();
            watch.Restart();
            var hashDb = HashDatabase.Open(hashDbPath,
                new HashDatabaseConfig {
                    Creation = CreatePolicy.NEVER,
                    ReadOnly = false,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                });
            watch.Stop();
            File.AppendAllText(_logTxt, "Open hash db from disk: " + watch.Elapsed + "\n");


            var cursor = hashDb.Cursor();
            var ids = new List<DatabaseEntry>();
            watch.Restart();

            while (cursor.MoveNext()) {
                ids.Add(cursor.Current.Key);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous load from hash after loading from disk: " + watch.Elapsed + "\n");


            watch.Restart();
            foreach (var id in ids.Shuffle()) {
                var result = hashDb.Get(id);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in hash after loading from disk: " + watch.Elapsed + "\n");


            hashDb.Close();
            hashDb.Dispose();

            GC.Collect();
        }


        static void TestBTreeLoadFromFile() {
            GC.Collect();

            var bTreeDbPath = Path.Combine(_home, "btree.bdb");

            var watch = new Stopwatch();
            watch.Restart();
            var bTreeDb = BTreeDatabase.Open(bTreeDbPath,
                new BTreeDatabaseConfig
                {
                    Creation = CreatePolicy.NEVER,
                    ReadOnly = false,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                });
            
            watch.Stop();
            File.AppendAllText(_logTxt, "Open hash db from disk: " + watch.Elapsed + "\n");


            var cursor = bTreeDb.Cursor();
            var ids = new List<DatabaseEntry>();
            watch.Restart();

            while (cursor.MoveNext()) {
                ids.Add(cursor.Current.Key);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous load from hash after loading from disk: " + watch.Elapsed + "\n");


            watch.Restart();
            foreach (var id in ids.Shuffle()) {
                var result = bTreeDb.Get(id);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in b-tree after loading from disk: " + watch.Elapsed + "\n");

            ids.Clear();



            bTreeDb.Close();
            bTreeDb.Dispose();
            GC.Collect();

        }


        static void TestHashDb(int length, byte[][] ids, byte[][] serializedEntities) {

            GC.Collect();

            var hashDbPath = Path.Combine(_home, "hash.bdb");
            if (File.Exists(hashDbPath))
                File.Delete(hashDbPath);

            var hashDb = HashDatabase.Open(hashDbPath,
                new HashDatabaseConfig {
                    Creation = CreatePolicy.IF_NEEDED,
                    ReadOnly = false,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                });


            var watch = new Stopwatch();
            watch.Restart();

            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var data = new DatabaseEntry(serializedEntities[i]);

                hashDb.Put(id, data);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Put in hash: " + watch.Elapsed + "\n");


            var resultPairs = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();

            watch.Restart();
            using (var cursor = hashDb.Cursor()) {
                while (cursor.MoveNext()) {
                    var current = cursor.Current;
                    resultPairs.Add(current);
                }
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous read from hash: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();


            watch.Restart();
            foreach (var idBytes in ids.Shuffle()) {
                var id = new DatabaseEntry(idBytes);
                var result = hashDb.Get(id);
                resultPairs.Add(result);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in hash: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();


/*
            var concurrentResults = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            watch.Restart();
            Parallel.ForEach(ids,
                bytes => {
                    var id = new DatabaseEntry(bytes);
                    var result = hashDb.Get(id);
                    concurrentResults.Add(result);
                });

            watch.Stop();
            File.AppendAllText(logTxt, "Parallel search by key in hash: " + watch.Elapsed + "\n");
            GC.Collect();
*/


            watch.Restart();
            hashDb.Sync();
            File.AppendAllText(_logTxt, "Hash sync: " + watch.Elapsed + "\n\n");

            hashDb.Close();
            hashDb.Dispose();
            GC.Collect();
        }
        
        static void TestHashDbEncrypted(int length, byte[][] ids, byte[][] serializedEntities) {

            GC.Collect();

            var hashDbPath = Path.Combine(_home, "hash_encrypted.bdb");
            if (File.Exists(hashDbPath))
                File.Delete(hashDbPath);


            var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
            };
            databaseEnvironmentConfig.SetEncryption("asdasd", EncryptionAlgorithm.AES);
            var env = DatabaseEnvironment.Open(_home, databaseEnvironmentConfig);
            

            var hashDb = HashDatabase.Open(hashDbPath,
                new HashDatabaseConfig {
                    Env = env,
                    Creation = CreatePolicy.IF_NEEDED,
                    Encrypted = true,
                    ReadOnly = false,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                });


            var watch = new Stopwatch();
            watch.Restart();

            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var data = new DatabaseEntry(serializedEntities[i]);

                hashDb.Put(id, data);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Put in encrypted hash: " + watch.Elapsed + "\n");

            var resultPairs = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();

            watch.Restart();
            using (var cursor = hashDb.Cursor()) {
                while (cursor.MoveNext()) {
                    var current = cursor.Current;
                    resultPairs.Add(current);
                }
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous read from encrypted hash: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();


            watch.Restart();
            foreach (var idBytes in ids.Shuffle()) {
                var id = new DatabaseEntry(idBytes);
                var result = hashDb.Get(id);
                resultPairs.Add(result);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in encrypted hash: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();


/*
            var concurrentResults = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            watch.Restart();
            Parallel.ForEach(ids,
                bytes => {
                    var id = new DatabaseEntry(bytes);
                    var result = hashDb.Get(id);
                    concurrentResults.Add(result);
                });

            watch.Stop();
            File.AppendAllText(logTxt, "Parallel search by key in hash: " + watch.Elapsed + "\n");
            GC.Collect();
*/


            watch.Restart();
            hashDb.Sync();
            File.AppendAllText(_logTxt, "Encrypted Hash sync: " + watch.Elapsed + "\n\n");

            hashDb.Close();
            hashDb.Dispose();
            GC.Collect();
        }
        

        static void TestBTreehDbMt(int length, byte[][] ids, SomeEntity[] entities, SomeOtherEntity[] otherEntities) {

            GC.Collect();

            var bTreeDbPath = Path.Combine(_home, "btree.bdb");
            if (File.Exists(bTreeDbPath))
                File.Delete(bTreeDbPath);


            var envOpen = new Func<DatabaseEnvironment>(() => {
                var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                    Create = true,
                    UseMPool = true,
                    SystemMemory = true,
                    Lockdown = true,
                   // ErrorPrefix = "Environment: ",
                   // ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                };
                databaseEnvironmentConfig.SetEncryption("asdasdd", EncryptionAlgorithm.AES);
                return DatabaseEnvironment.Open(_home, databaseEnvironmentConfig);
            });

            var entitiesDb = new ThreadLocalDb<BTreeDatabase>(envOpen, env => OpenBTreeDb(bTreeDbPath, nameof(SomeEntity), env));

            var otherEntitiesDb = new ThreadLocalDb<BTreeDatabase>(envOpen, env => OpenBTreeDb(bTreeDbPath, nameof(SomeOtherEntity), env)); // primary

            
            //var foreignKey = new ThreadLocalDb<SecondaryBTreeDatabase>(envOpen, env => MakeForeingKey(bTreeDbPath, otherEntitiesDb.Instance(), entitiesDb.Instance(), env));
            // foreign key and constraint
                                                                                                                        
            
                      
            //var foreignKey = MakeForeingKey(bTreeDbPath, otherEntitiesDb, entitiesDb, env); // foreign key and constraint


            /*
                        var foreignFields = new [] {nameof(SomeEntity.Name), nameof(SomeEntity.Value)};
                        var indexes = new List<SecondaryBTreeDatabase>();
                        foreach (var foreignField in foreignFields) {


                            indexes.Add(SecondaryBTreeDatabase.Open(bTreeDbPath,
                                $"{nameof(SomeEntity)}_{foreignField}",
                                new SecondaryBTreeDatabaseConfig(bTreeDb,
                                    (key, data) => {
                                        var entity = SomeEntity.Deserialize(data.Data);
                                        object value;
                                        if (!RUtils<SomeEntity>.TryGetValueOnPath(entity, foreignField, out value))
                                            return null;

                                        return new DatabaseEntry(Encoding.UTF8.GetBytes((string) value));
                                    }) {
                                        Env = env,
                                        Creation = CreatePolicy.IF_NEEDED,
                                        ErrorPrefix = $"Secondary '{foreignField}': ",
                                        ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                                    }));
                        }
            */



            var watch = new Stopwatch();
            watch.Restart();
            Parallel.For(0, length, i => {
                var id = new DatabaseEntry(ids[i]);
                var text = entities[i].ToJson();
                var data = new DatabaseEntry(Encoding.UTF8.GetBytes(text));
                entitiesDb.Instance().Put(id, data);

                var other1 = otherEntities[i*2];
                var other1Id = new DatabaseEntry(other1.Id.ToByteArray());
                var other1Data = new DatabaseEntry(Encoding.UTF8.GetBytes(other1.ToJson()));
                otherEntitiesDb.Instance().Put(other1Id, other1Data);

                var other2 = otherEntities[i*2 + 1];
                var other2Id = new DatabaseEntry(other2.Id.ToByteArray());
                var other2Data = new DatabaseEntry(Encoding.UTF8.GetBytes(other2.ToJson()));
                otherEntitiesDb.Instance().Put(other2Id, other2Data);
            });

            watch.Stop();
            File.AppendAllText(_logTxt, "Put in b-tree in parallel: " + watch.Elapsed + "\n");
            
            entitiesDb.Sync();
            otherEntitiesDb.Sync();


            

            var resultPairs = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
/*

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
*/



            watch.Restart();
            Parallel.For(0,
                ids.Length,
                i => {
                    var id = new DatabaseEntry(ids[i]);
                    var result = entitiesDb.Instance().Get(id);
                    resultPairs.Add(result);
                });
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in entities b-tree in parallel: " + watch.Elapsed + "\n");

            resultPairs = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            GC.Collect();

            watch.Restart();
            Parallel.For(0,
                otherEntities.Length,
                i => {
                    var id = new DatabaseEntry(otherEntities[i].Id.ToByteArray());
                    var result = otherEntitiesDb.Instance().Get(id);
                    resultPairs.Add(result);
                });

            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in other entities b-tree in parallel: " + watch.Elapsed + "\n");


            resultPairs = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            GC.Collect();

/*
            watch.Restart();
            Parallel.For(0,
                ids.Length,
                i => {

                    var foreignCursor = otherEntitiesDb.Foreign().Cursor();

                    var someEntityId = new DatabaseEntry(ids[i]);

                    if (!foreignCursor.Move(someEntityId, exact: true)) {
                        Console.WriteLine($"{someEntityId} is not found in foreign database");
                        return;
                    }

                    resultPairs.Add(foreignCursor.Current);

                    while (foreignCursor.MoveNextDuplicate()) {
                        resultPairs.Add(foreignCursor.Current);
                    }

                    foreignCursor.Close();
                });
*/
/*

            watch.Stop();
            File.AppendAllText(_logTxt, "Search other entity by id of entity (by foreign key) in b-tree in parallel: " + watch.Elapsed + "\n");

            resultPairs = new ConcurrentBag<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            GC.Collect();*/
            


            entitiesDb.Close();

            otherEntitiesDb.Close();


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
            #1#


            watch.Restart();
            entitiesDb.Sync();
            otherEntitiesDb.Sync();
            foreignKey.Sync();
            File.AppendAllText(_logTxt, "b-tree sync: " + watch.Elapsed + "\n\n");

/*            foreach (var index in indexes) {
                index.Close();
            }#1#
            
            foreignKey.Close();
            otherEntitiesDb.Close();
            otherEntitiesDb.Dispose();
            entitiesDb.Close();
            entitiesDb.Dispose();
            GC.Collect();*/
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


/*
            var foreignFields = new [] {nameof(SomeEntity.Name), nameof(SomeEntity.Value)};
            var indexes = new List<SecondaryBTreeDatabase>();
            foreach (var foreignField in foreignFields) {


                indexes.Add(SecondaryBTreeDatabase.Open(bTreeDbPath,
                    $"{nameof(SomeEntity)}_{foreignField}",
                    new SecondaryBTreeDatabaseConfig(bTreeDb,
                        (key, data) => {
                            var entity = SomeEntity.Deserialize(data.Data);
                            object value;
                            if (!RUtils<SomeEntity>.TryGetValueOnPath(entity, foreignField, out value))
                                return null;

                            return new DatabaseEntry(Encoding.UTF8.GetBytes((string) value));
                        }) {
                            Env = env,
                            Creation = CreatePolicy.IF_NEEDED,
                            ErrorPrefix = $"Secondary '{foreignField}': ",
                            ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                        }));
            }
*/



            var watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var text = entities[i].ToJson();
                var data = new DatabaseEntry(Encoding.UTF8.GetBytes(text));
                entitiesDb.Put(id, data);

                var other1 = otherEntities[i*2];
                var other1Id = new DatabaseEntry(other1.Id.ToByteArray());
                var other1Data = new DatabaseEntry(Encoding.UTF8.GetBytes(other1.ToJson()));
                otherEntitiesDb.Put(other1Id, other1Data);

                var other2 = otherEntities[i*2 +1];
                var other2Id = new DatabaseEntry(other2.Id.ToByteArray());
                var other2Data = new DatabaseEntry(Encoding.UTF8.GetBytes(other2.ToJson()));
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
                    var text = Encoding.UTF8.GetString(pData.Data);
                    var otherEntity = text.FromJson<SomeOtherEntity>();

                    var secondaryId = otherEntity.SomeEntityId.ToByteArray();
                    return new DatabaseEntry(secondaryId);
                }) {
                    Env = env,
                    Encrypted = env?.EncryptAlgorithm == EncryptionAlgorithm.AES,
                    Duplicates = DuplicatesPolicy.SORTED,
                    Creation = CreatePolicy.IF_NEEDED,
                    ErrorPrefix = $"{otherEntitiesDb.DatabaseName}->{entitiesDb.DatabaseName} Db:",
                    ErrorFeedback = (prefix, message) => 
                    File.AppendAllText(_logTxt, prefix + message + "\n")
                };

            foreignKeyConfig.SetForeignKeyConstraint(entitiesDb, ForeignKeyDeleteAction.ABORT);


            var secondary = SecondaryBTreeDatabase.Open(bTreeDbPath, $"{otherEntitiesDb.DatabaseName}->{entitiesDb.DatabaseName}", foreignKeyConfig);
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

        

        static void TestBTreehDbEncrypted(int length, byte[][] ids, byte[][] serializedEntities) {

            GC.Collect();


            var bTreeDbPath = Path.Combine(_home, "btree_encrypted.bdb");
            if (File.Exists(bTreeDbPath))
                File.Delete(bTreeDbPath);


            var databaseEnvironmentConfig = new DatabaseEnvironmentConfig {
                Create = true,
                UseMPool = true,
            };
            databaseEnvironmentConfig.SetEncryption("asdasd", EncryptionAlgorithm.AES);
            var env = DatabaseEnvironment.Open(_home, databaseEnvironmentConfig);

            var bTreeDb = BTreeDatabase.Open(bTreeDbPath,
                new BTreeDatabaseConfig {
                    Env = env,
                    Encrypted = true,
                    Creation = CreatePolicy.IF_NEEDED,
                    ReadOnly = false,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(_logTxt, prefix + message + "\n"),
                });


            var watch = new Stopwatch();
            watch.Restart();

            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var data = new DatabaseEntry(serializedEntities[i]);

                bTreeDb.Put(id, data);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Put in encrypted b-tree: " + watch.Elapsed + "\n");

            var resultPairs = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();

            watch.Restart();
            using (var cursor = bTreeDb.Cursor()) {
                while (cursor.MoveNext()) {
                    var current = cursor.Current;
                    resultPairs.Add(current);
                }
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Continuous read from encrypted b-tree: " + watch.Elapsed + "\n");

            resultPairs.Clear();
            GC.Collect();


            watch.Restart();
            foreach (var idBytes in ids.Shuffle()) {
                var id = new DatabaseEntry(idBytes);
                var result = bTreeDb.Get(id);
                resultPairs.Add(result);
            }
            watch.Stop();
            File.AppendAllText(_logTxt, "Search by key in encrypted b-tree: " + watch.Elapsed + "\n");

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
            bTreeDb.Sync();
            File.AppendAllText(_logTxt, "Encrypted b-tree sync: " + watch.Elapsed + "\n\n");

            bTreeDb.Close();
            bTreeDb.Dispose();
            GC.Collect();
        }
        

    }
}
