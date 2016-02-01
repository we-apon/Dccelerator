using System.Collections.Generic;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbRepository {

        IEnumerable<DatabaseEntry> ContinuouslyReadToEnd(string entityName);

        IEnumerable<DatabaseEntry> GetByKeyFromPrimaryDb(DatabaseEntry key, string entityName);

        IEnumerable<DatabaseEntry> GetFromSecondaryDb(DatabaseEntry key, string entityName, string secondarySubName, DuplicatesPolicy duplicatesPolicy);

        bool Insert(object entity, string name, ICollection<ForeignKeyAttribute> mappings);
    }
}