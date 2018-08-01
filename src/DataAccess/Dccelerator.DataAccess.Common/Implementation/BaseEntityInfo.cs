using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.UnFastReflection;


namespace Dccelerator.DataAccess.Implementation {

    public abstract class BaseEntityInfo<TRepository> where TRepository : class {

        protected BaseEntityInfo(Type entityType) {
            EntityType = entityType;
            EntityName = entityType.Name;
            CacheTimeout = TimeSpan.Zero;

            var table = entityType.Name;
            var lastChar = table.Last();
            table += char.ToLowerInvariant(lastChar).Equals('s') ? "es" : "s";
            TableName = table;

            var entityAttribute = entityType.GetConfigurationForRepository(typeof(TRepository));
            if (entityAttribute != null) {
                EntityName = entityAttribute.Name ?? entityType.Name;
                UsingQueries = entityAttribute.UseQueries;
                TableName = entityAttribute.TableName ?? table;

                if (entityAttribute.Repository != null)
                    Repository = Activator.CreateInstance(entityAttribute.Repository) as TRepository;

                if (entityAttribute is GloballyCachedEntityAttribute cachedEntityAttribute) {
                    CacheTimeout = cachedEntityAttribute.Timeout;
                    IsGloballyCached = true;
                }
            }

        }


        public bool UsingQueries { get; set; }

        public bool IsGloballyCached { get; set; }

        public TRepository Repository { get; set; }


#if NET40
        public Type TypeInfo => EntityType;

#else
        public TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = EntityType.GetInfo());
        private TypeInfo _typeInfo;
#endif



#region Implementation of IEntityInfo

        public virtual string TableName { get; }
        public virtual string EntityName { get; }
        public virtual Type EntityType { get; }
        public TimeSpan CacheTimeout { get; }



        public virtual Dictionary<string, ForeignKeyAttribute> ForeignKeys {
            get {
                if (_foreignKeys == null) {
                    lock (_lock) {
                        if (_foreignKeys == null)
                            _foreignKeys = EntityInfoBaseBackend.Get<ForeignKeyAttribute>(TypeInfo);
                    }
                }
                return _foreignKeys;
            }
        }
        Dictionary<string, ForeignKeyAttribute> _foreignKeys;




        public virtual Dictionary<string, SecondaryKeyAttribute> SecondaryKeys {
            get {
                if (_secondaryKeys == null) {
                    lock (_lock) {
                        if (_secondaryKeys == null)
                            _secondaryKeys = EntityInfoBaseBackend.Get<SecondaryKeyAttribute>(TypeInfo);
                    }
                }
                return _secondaryKeys;
            }
        }
        Dictionary<string, SecondaryKeyAttribute> _secondaryKeys;

        


        public virtual Dictionary<string, PropertyInfo> PersistedProperties {
            get {
                if (_persistedProperties == null) {
                    lock (_lock) {
                        if (_persistedProperties == null)
                            _persistedProperties = EntityType.Properties(BindingFlags.Instance | BindingFlags.Public, IsPersistedProperty);
                    }
                }
                return _persistedProperties;
            }
        }
        Dictionary<string, PropertyInfo> _persistedProperties;

        protected virtual Func<PropertyInfo, bool> IsPersistedProperty { get; } = EntityInfoBaseBackend.IsPersistedProperty;

        #endregion

        readonly object _lock = new object();

    }
}