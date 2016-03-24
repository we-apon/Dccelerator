using System;
using System.Collections.Generic;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbSchema : IDisposable {

        Database GetPrimaryDb(string dbName, bool readOnly);

        SecondaryDatabase GetReadOnlySecondaryDb(Database primaryDb, SecondaryKeyAttribute secondaryKey);

        SecondaryDatabase GetWritableForeignKeyDatabase(Database primaryDb, Database foreignDb, SecondaryKeyAttribute foreignKeyMapping);

        Transaction BeginTransactionFor(ICollection<IBDbEntityInfo> entityInfos);

    }
}