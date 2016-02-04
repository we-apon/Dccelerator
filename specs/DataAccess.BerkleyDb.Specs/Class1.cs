using System; 
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using BerkeleyDB;
using Dccelerator;
using Machine.Specifications;
// ReSharper disable InconsistentNaming


namespace DataAccess.BerkleyDb.Specs
{
    public class Inserting_entities_into_Berkley_Db {
        Because of = () => {
            var hashDbPath = Path.Combine(_home, "hash.bdb");
            var bTreeDbPath = Path.Combine(_home, "btree.bdb");
            var ranchoDbPath = Path.Combine(_home, "rancho.bdb");
            var logTxt = Path.Combine(_home, "log.txt");

            var environmentHash = DatabaseEnvironment.Open(_home,
                new DatabaseEnvironmentConfig {
                    Create = true,
                    Private = true,
                    UseMPool = true,
                    ErrorPrefix = "Env: ",
                    ErrorFeedback = (pfx, msg) => File.AppendAllText(logTxt, pfx + msg + "\n"),
                });
            


            var hashDb = HashDatabase.Open(hashDbPath,
                new HashDatabaseConfig {
                    Env = environmentHash,
                    Creation = CreatePolicy.NEVER,
                    ReadOnly = false,
                    ReadUncommitted = true,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(logTxt, prefix + message + "\n"),
                });



            var environmentBtree = DatabaseEnvironment.Open(_home,
                new DatabaseEnvironmentConfig
                {
                    Create = true,
                    Private = true,
                    UseMPool = true,
                    ErrorPrefix = "Env: ",
                    ErrorFeedback = (pfx, msg) => File.AppendAllText(logTxt, pfx + msg + "\n"),
                });



            var bTreeDb = BTreeDatabase.Open(bTreeDbPath,
                new BTreeDatabaseConfig {
                    Env = environmentBtree,
                    Creation = CreatePolicy.IF_NEEDED,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(logTxt, prefix + message + "\n"),
                });

            var ranchoBtreeEnvir = DatabaseEnvironment.Open(_home,
                new DatabaseEnvironmentConfig
                {
                    Create = true,
                    Private = true,
                    UseMPool = true,
                    ErrorPrefix = "Env: ",
                    ErrorFeedback = (pfx, msg) => File.AppendAllText(logTxt, pfx + msg + "\n"),
                });



            var ranchoDb = RecnoDatabase.Open(ranchoDbPath,
                new RecnoDatabaseConfig {
                    Env = ranchoBtreeEnvir,
                    Creation = CreatePolicy.IF_NEEDED,
                    ErrorPrefix = "Db: ",
                    ErrorFeedback = (prefix, message) => File.AppendAllText(logTxt, prefix + message + "\n"),
                });


/*

            using (var cursor = hashDb.Cursor())
            {
                while (cursor.MoveNext())
                {
                    var current = cursor.Current;

                    var entity = SomeEntity.Deserialize(current.Value.Data);
                    _results.Add(entity);
                }
            }

*/

            File.AppendAllText(logTxt, "Start!\n");

            var length = 1000000;
            var entities = new SomeEntity[length];
            var ids = new byte[length][];
            var serializedEntities = new byte[length][];
            Parallel.For(0,
                length,
                i => {
                    var someEntity = RandomMaker.Make<SomeEntity>();
                    entities[i] = someEntity;
                    serializedEntities[i] = someEntity.Serialize();
                    ids[i] = someEntity.Id.ToByteArray();
                });


            var watch = new Stopwatch();
            watch.Restart();
            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var data = new DatabaseEntry(serializedEntities[i]);

                hashDb.Put(id, data);
            }
            watch.Stop();
            File.AppendAllText(logTxt, "Put in hash  : " + watch.Elapsed + "\n");

            watch.Restart();
            hashDb.Sync();
            watch.Stop();
            File.AppendAllText(logTxt, "Sync in hash  : " + watch.Elapsed + "\n");



            watch.Restart();
            for (int i = 0; i < length; i++) {
                var data = new DatabaseEntry(serializedEntities[i]);

                ranchoDb.Append(data);
            }
            watch.Stop();
            File.AppendAllText(logTxt, "Put in Recno  : " + watch.Elapsed + "\n");



            watch.Restart();
            ranchoDb.Sync();
            watch.Stop();
            File.AppendAllText(logTxt, "Sync in Recno  : " + watch.Elapsed + "\n");



            watch.Restart();
            for (int i = 0; i < length; i++) {
                var id = new DatabaseEntry(ids[i]);
                var data = new DatabaseEntry(serializedEntities[i]);

                bTreeDb.Put(id, data);
            }
            watch.Stop();
            File.AppendAllText(logTxt, "Put in b-tree: " + watch.Elapsed + "\n");


            watch.Restart();
            bTreeDb.Sync();
            watch.Stop();
            File.AppendAllText(logTxt, "Sync in b-tree: " + watch.Elapsed + "\n");


/*

            var loadHash = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            var loadBTree = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();



            watch.Restart();
            foreach (var entity in entities) {
                var id = new DatabaseEntry(entity.Id.ToByteArray());
                var pair = hashDb.Get(id);
                loadHash.Add(pair);
            }
            File.AppendAllText(logTxt, "Load from hash  : " + watch.Elapsed + "\n");
            File.AppendAllText(logTxt, "Load from hash count is : " + loadHash.Count + "\n");


            watch.Restart();
            foreach (var entity in entities) {
                var id = new DatabaseEntry(entity.Id.ToByteArray());
                var pair = bTreeDb.Get(id);
                loadBTree.Add(pair);
            }
            File.AppendAllText(logTxt, "Load from b-tree: " + watch.Elapsed + "\n");
            File.AppendAllText(logTxt, "Load from b-tree count is: " + loadBTree.Count + "\n");
*/



/*            foreach (var entity in entities) {
                var serializedEntity = entity.Serialize();

                var id = new DatabaseEntry(entity.Id.ToByteArray());
                var data = new DatabaseEntry(serializedEntity);

                hashDb.Put(id, data);
            }



            using (var cursor = hashDb.Cursor()) {
                while (cursor.MoveNext()) {
                    var current = cursor.Current;

                    var entity = SomeEntity.Deserialize(current.Value.Data);
                    _results.Add(entity);
                }
            }


            foreach (var entity in _entities) {

                var id = new DatabaseEntry(entity.Id.ToByteArray());

                var resultedData = hashDb.Get(id);

                var resultedEntity = SomeEntity.Deserialize(resultedData.Value.Data);

                resultedEntity.Id.ShouldEqual(entity.Id);
                resultedEntity.Name.ShouldEqual(entity.Name);
                resultedEntity.Value.ShouldEqual(entity.Value);

                id.ShouldEqual(resultedData.Key);
            }*/

            hashDb.Sync();
            hashDb.Close();
            environmentHash.Close();

            bTreeDb.Sync();
            bTreeDb.Close();
            environmentBtree.Close();



            ranchoDb.Sync();
            ranchoDb.Close();
            ranchoBtreeEnvir.Close();
        };

        It should_work = () => {

        };




        static readonly string _home = AppDomain.CurrentDomain.BaseDirectory;

        static List<SomeEntity> _results = new List<SomeEntity>();

        static readonly List<SomeEntity> _entities = new List<SomeEntity> {
            RandomMaker.Make<SomeEntity>(includeGuids:true),
            RandomMaker.Make<SomeEntity>(includeGuids:true),
            RandomMaker.Make<SomeEntity>(includeGuids:true),
            RandomMaker.Make<SomeEntity>(includeGuids:true),
            RandomMaker.Make<SomeEntity>(includeGuids:true),
            RandomMaker.Make<SomeEntity>(includeGuids:true),
        };
    }



    [XmlRoot]
    public class SomeEntity {
        /// <summary>
        /// Id of <see cref="SomeEntity"/>
        /// </summary>
        [XmlElement]
        public Guid Id { get; set; }


        /// <summary>
        /// Name of <see cref="SomeEntity"/>
        /// </summary>
        [XmlElement]
        public string Name { get; set; }


        /// <summary>
        /// Value of <see cref="SomeEntity"/>
        /// </summary>
        [XmlElement]
        public string Value { get; set; }


        public byte[] Serialize() {
            var serializer = new XmlSerializer(typeof(SomeEntity));
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, this);
                writer.Flush();
                return Encoding.UTF8.GetBytes(writer.ToString());
            }
        }


        public static SomeEntity Deserialize(byte[] bytes) {
            var text = Encoding.UTF8.GetString(bytes);

            var serializer = new XmlSerializer(typeof(SomeEntity));

            using (var reader = new StringReader(text))
            using (var xmlReader = XmlReader.Create(reader)) {
                return serializer.CanDeserialize(xmlReader)
                    ? serializer.Deserialize(xmlReader) as SomeEntity
                    : null;
            }



        }
    }

}
