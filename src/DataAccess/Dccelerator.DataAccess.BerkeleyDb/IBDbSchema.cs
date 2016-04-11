using System;
using System.Collections.Generic;
using BerkeleyDB;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbSchema : IDisposable {

        Database GetPrimaryDb(string dbName);

        SecondaryDatabase GetSecondaryDb(Database primaryDb, SecondaryKeyAttribute foreignKeyMapping, [CanBeNull] Database foreignDb = null);

        Transaction BeginTransactionFor(ICollection<IBDbEntityInfo> entityInfos);

    }
}