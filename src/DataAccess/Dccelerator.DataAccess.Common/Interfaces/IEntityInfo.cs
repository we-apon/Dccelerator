using System;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess {
    public interface IEntityInfo {
        string EntityName { get; }
        Type EntityType { get; }

        TimeSpan CacheTimeout { get; }

        Dictionary<string, ForeignKeyAttribute> ForeignKeys { get; }

        Dictionary<string, SecondaryKeyAttribute> SecondaryKeys { get; }

        Dictionary<string, Type> PersistedProperties { get; }

        Dictionary<string, Type> NavigationProperties { get; }

        [CanBeNull]
        IEnumerable<IIncludeon> Inclusions { get; }
    }
}