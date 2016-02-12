using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Attributes;

#if !NET40
using Dccelerator.Reflection;
#endif

namespace Dccelerator.DataAccess {

    public abstract class BaseEntityInfo<TRepository> where TRepository : class {


        protected BaseEntityInfo(Type entityType) {
            EntityType = entityType;
            EntityName = entityType.Name;
            CacheTimeout = TimeSpan.Zero;

            var entityAttribute = entityType.GetConfigurationForRepository(typeof(TRepository));
            if (entityAttribute != null) {
                EntityName = entityAttribute.Name ?? EntityType.Name;
                Repository = Activator.CreateInstance(entityAttribute.Repository) as TRepository;

                var cachedEntityAttribute = entityAttribute as GloballyCachedEntityAttribute;
                if (cachedEntityAttribute != null)
                    CacheTimeout = cachedEntityAttribute.Timeout;
            }
        }


        public TRepository Repository { get; set; }


#if NET40
        public Type TypeInfo => EntityType;

#else
        public TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = EntityType.GetInfo());
        private TypeInfo _typeInfo;
#endif



#region Implementation of IEntityInfo

        public virtual string EntityName { get; }
        public virtual Type EntityType { get; }
        public TimeSpan CacheTimeout { get; }

        readonly object _foreignKeysLock = new object();
        Dictionary<string, ForeignKeyAttribute> _foreignKeys;


        public virtual Dictionary<string, ForeignKeyAttribute> ForeignKeys {
            get {
                if (_foreignKeys == null) {
                    lock (_foreignKeysLock) {
                        if (_foreignKeys == null)
                            _foreignKeys = GetForeignKeysOf(TypeInfo);
                    }
                }
                return _foreignKeys;
            }
        }
/*
        public abstract Dictionary<string, SecondaryKeyAttribute> SecondaryKeys { get; }
        public abstract Dictionary<string, Type> PersistedProperties { get; }
        public abstract Dictionary<string, Type> NavigationProperties { get; }*/

        #endregion






#if NET40
        Dictionary<string, ForeignKeyAttribute> GetForeignKeysOf(Type typeInfo) {
#else
        Dictionary<string, ForeignKeyAttribute> GetForeignKeysOf(TypeInfo typeInfo) {
#endif
            var dict = new Dictionary<string, ForeignKeyAttribute>();

            var properties = typeInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var property in properties) {
                var foreignKeyAttributes = property.GetCustomAttributes(ForeignKeyAttribute.Type, inherit: true).Cast<ForeignKeyAttribute>().ToList();
                if (foreignKeyAttributes.Count < 1)
                    continue;

                if (foreignKeyAttributes.Count > 1)
                    throw new InvalidOperationException($"Property {typeInfo.FullName}.{property.Name} contains more that one {nameof(ForeignKeyAttribute)}.");

                var attribute = foreignKeyAttributes.Single();
                if (string.IsNullOrWhiteSpace(attribute.ForeignKeyPath))
                    attribute.ForeignKeyPath = property.Name;

                dict.Add(property.Name, attribute);
            }

            return dict;
        }
    }
}