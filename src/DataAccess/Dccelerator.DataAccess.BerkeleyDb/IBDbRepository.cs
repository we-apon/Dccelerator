using System.Collections.Generic;
using BerkeleyDB;
using Dccelerator.DataAccess.BerkeleyDb.Implementation;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbRepository {

        bool IsPrimaryKey(IDataCriterion criterion);

        DatabaseEntry EntryFrom(IDataCriterion criterion);

        IEnumerable<DatabaseEntry> ContinuouslyReadToEnd(string entityName);

        IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName);

        IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, SecondaryKeyAttribute secondaryKey);

        IEnumerable<DatabaseEntry> GetByJoin(IBDbEntityInfo info, ICollection<IDataCriterion> criteria);

        bool PerformInTransaction(ICollection<IBDbEntityInfo> entityInfos, IEnumerable<TransactionElement> elements);
    }
}
