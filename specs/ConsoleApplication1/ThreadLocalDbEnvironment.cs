using System;
using System.Threading;
using BerkeleyDB;


namespace ConsoleApplication1 {
    public class ThreadLocalDb<TDatabase> where TDatabase : BaseDatabase {
        readonly ThreadLocal<TDatabase> _database;
        readonly ThreadLocal<DatabaseEnvironment> _environment;


        public ThreadLocalDb(Func<DatabaseEnvironment> environmentOpen, Func<DatabaseEnvironment, TDatabase> databaseOpen) {
            _environment = new ThreadLocal<DatabaseEnvironment>(environmentOpen, true);
            _database = new ThreadLocal<TDatabase>(() => databaseOpen(_environment.Value), true);
        }
        


        public TDatabase Instance() {
            return _database.Value;
        }




        public void Sync() {
            foreach (var database in _database.Values) {
                database.Sync();
            }
        }


        public void Close() {
            
            foreach (var database in _database.Values) {
                database.Close();
            }


            foreach (var env in _environment.Values) {
                env.Close();
            }
        }

    }
}