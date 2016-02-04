using System.Collections.Generic;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbRepository {

        bool IsPrimaryKey(IDataCriterion criterion);

        DatabaseEntry EntryFrom(IDataCriterion criterion);

        IEnumerable<DatabaseEntry> ContinuouslyReadToEnd(string entityName);

        IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName);

        IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, string indexSubName, DuplicatesPolicy duplicatesPolicy);

        IEnumerable<DatabaseEntry> GetByJoin(string entityName, ICollection<IDataCriterion> criteria, IBDbEntityInfo info);

        bool PerformInTransaction(ICollection<IBDbEntityInfo> entityInfos, IEnumerable<TransactionElement> elements);
    }
}
