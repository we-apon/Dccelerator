using System;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbSchema : IDisposable {
        Database GetPrimaryDb(string dbName, bool readOnly);
        SecondaryDatabase GetReadOnlySecondaryDb(Database primaryDb, string indexSubName, DuplicatesPolicy duplicatesPolicy);
        SecondaryDatabase GetWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute foreignKeyMapping);
    }
}