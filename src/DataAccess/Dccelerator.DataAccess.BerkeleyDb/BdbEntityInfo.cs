using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Dccelerator.DataAccess.Attributes;

#if !NET40
using Dccelerator.Reflection;
#endif


namespace Dccelerator.DataAccess.BerkeleyDb {
    class BDbEntityInfo : IBDbEntityInfo {

        public string EntityName { get; set; }

        public Type EntityType { get; set; }

#if NET40
        public Type TypeInfo => EntityType;

#else
        public TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = Type.GetInfo());
        private TypeInfo _typeInfo;
#endif


        readonly object _lock = new object();
        Dictionary<string, ForeignKeyAttribute> _mappings;

        public Dictionary<string, ForeignKeyAttribute> ForeignKeys {
            get {
                if (_mappings == null) {
                    lock (_lock) {
                        if (_mappings == null)
                            _mappings = GetMappingsOf(TypeInfo);
                    }
                }

                return _mappings;
            }
        }


        public Dictionary<string, SecondaryKeyAttribute> SecondaryKeys { get {throw new NotImplementedException(); } }
        public Dictionary<string, Type> PersistedProperties { get {throw new NotImplementedException(); } }


#if NET40
        Dictionary<string, ForeignKeyAttribute> GetMappingsOf(Type typeInfo) {
            var dict = new Dictionary<string, ForeignKeyAttribute>();

            var properties = typeInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var property in properties) {
                var foreignKeyAttributes = property.GetCustomAttributes(ForeignKeyAttribute.Type, inherit:true).Cast<ForeignKeyAttribute>().ToList();
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
#else
        Dictionary<string, ForeignKeyAttribute> GetMappingsOf(TypeInfo typeInfo) {
            throw new NotImplementedException();
        }
#endif




        public IBDbRepository Repository { get; set; }


        static readonly Type _bdbRepositoryType = typeof (IBDbRepository);

        public BDbEntityInfo(Type type) {
            EntityType = type;

            var entityAttribute = type.GetConfigurationForRepository(_bdbRepositoryType);
            if (entityAttribute != null) {
                EntityName = entityAttribute.Name ?? EntityType.Name;
                Repository = Activator.CreateInstance(entityAttribute.Repository) as IBDbRepository;
            }
            else {
                EntityName = EntityType.Name;
            }
        }


        #region Overrides of Object

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj) {
            return Equals(obj as IBDbEntityInfo);
        }


        #region Equality members

        protected bool Equals(IBDbEntityInfo other) {
            return string.Equals(EntityName, other.EntityName) && Equals(EntityType, other.EntityType) && Equals(Repository, other.Repository);
        }


        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                var hashCode = EntityName?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (EntityType?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Repository?.GetHashCode() ?? 0);
                return hashCode;
            }
        }


        public static bool operator ==(BDbEntityInfo left, BDbEntityInfo right) {
            return Equals(left, right);
        }


        public static bool operator !=(BDbEntityInfo left, BDbEntityInfo right) {
            return !Equals(left, right);
        }

        #endregion


        #endregion
    }
}