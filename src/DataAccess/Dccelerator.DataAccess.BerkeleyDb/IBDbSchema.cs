using System;
using System.Collections.Generic;
using BerkeleyDB;
using Dccelerator.DataAccess.Attributes;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbSchema : IDisposable {

        Database GetPrimaryDb(string dbName, bool readOnly);

        SecondaryDatabase GetReadOnlySecondaryDb(Database primaryDb, string indexSubName, DuplicatesPolicy duplicatesPolicy);

        SecondaryDatabase GetWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, ForeignKeyAttribute foreignKeyMapping);

        Transaction BeginTransactionFor(ICollection<IBDbEntityInfo> entityInfos);

    }
}