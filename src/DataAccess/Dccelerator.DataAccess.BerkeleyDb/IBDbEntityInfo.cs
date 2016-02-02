using System;
using System.Collections.Generic;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbEntityInfo {
        string EntityName { get; }
        Type Type { get; }
        Dictionary<string, ForeignKeyAttribute> ForeignKeys { get; }
        IBDbRepository Repository { get; }
    }
}