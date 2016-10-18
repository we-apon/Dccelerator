using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;

namespace Dccelerator.DataAccess.Implementation {

    public abstract class BaseEntityInfo<TRepository> where TRepository : class {

        protected BaseEntityInfo(Type entityType) {
            EntityType = entityType;
            EntityName = entityType.Name;
            CacheTimeout = TimeSpan.Zero;

            var entityAttribute = entityType.GetConfigurationForRepository(typeof(TRepository));
            if (entityAttribute != null) {
                EntityName = entityAttribute.Name ?? EntityType.Name;
                Repository = entityAttribute.Repository?.CreateInstance() as TRepository;
                UsingQueries = entityAttribute.UseQueries;

                var cachedEntityAttribute = entityAttribute as GloballyCachedEntityAttribute;
                if (cachedEntityAttribute != null) {
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



    class EntityInfoBaseBackend {
        
#if NET40
        internal static Dictionary<string, TAttribute> Get<TAttribute>(Type typeInfo) where TAttribute : SecondaryKeyAttribute {
#else
        internal static Dictionary<string, TAttribute> Get<TAttribute>(TypeInfo typeInfo) where TAttribute : SecondaryKeyAttribute {
#endif

            var dict = new Dictionary<string, TAttribute>();

            var properties = typeInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var attributeType = RUtils<TAttribute>.Type;

            foreach (var property in properties) {
                var keyAttributes = property.GetCustomAttributes(attributeType, inherit: true).Cast<TAttribute>().ToList();
                if (keyAttributes.Count < 1)
                    continue;

                if (keyAttributes.Count > 1)
                    throw new InvalidOperationException($"Property {typeInfo.FullName}.{property.Name} contains more that one {attributeType.Name}.");

                var attribute = keyAttributes.Single();
                if (string.IsNullOrWhiteSpace(attribute.Name))
                    attribute.Name = property.Name;

                dict.Add(property.Name, attribute);
            }

            return dict;
        }





        static readonly Type _notPersistedAttributeType = typeof(NotPersistedAttribute);
        static readonly Type _stringType = typeof(string);
        static readonly Type _byteArrayType = typeof(byte[]);
        static readonly Type _enumerableType = typeof(IEnumerable);




        internal static readonly Func<PropertyInfo, bool> IsPersistedProperty = property => {

            //? if marked with NotPersistedAttribute
            if (property.GetCustomAttributesData().Any(x => x.AttributeType() == _notPersistedAttributeType))
                return false;

            if (!property.CanRead)
                return false;

            var type = property.PropertyType;

            if (type == _stringType || type.IsAssignableFrom(_byteArrayType))
                return true;

            if (_enumerableType.IsAssignableFrom(type) || type.GetInfo().IsClass)
                return false;

            return true;
        };
    }
}