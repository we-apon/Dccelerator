using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess {
    public interface IEntityInfo {
        string EntityName { get; }
        Type EntityType { get; }

        TimeSpan CacheTimeout { get; }

        Dictionary<string, ForeignKeyAttribute> ForeignKeys { get; }

        Dictionary<string, SecondaryKeyAttribute> SecondaryKeys { get; }

        Dictionary<string, PropertyInfo> PersistedProperties { get; }

        Dictionary<string, PropertyInfo> NavigationProperties { get; }

        [CanBeNull]
        IEnumerable<IIncludeon> Inclusions { get; }
    }
}