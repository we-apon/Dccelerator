using System;
using System.Collections.Generic;


namespace Dccelerator.DataAccess {
    public interface IEntityInfo {
        string EntityName { get; }
        Type Type { get; }
        Dictionary<string, ForeignKeyAttribute> ForeignKeys { get; }
    }
}