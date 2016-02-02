using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Attributes;


namespace Dccelerator.DataAccess.BerkeleyDb {
    class BDbEntityInfo : IBDbEntityInfo {

        public string EntityName { get; set; }

        public Type Type { get; set; }

#if NET40
        public Type TypeInfo => Type;

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
            Type = type;

            var entityAttribute = type.GetConfigurationForRepository(_bdbRepositoryType);
            if (entityAttribute != null) {
                EntityName = entityAttribute.Name ?? Type.Name;
                Repository = Activator.CreateInstance(entityAttribute.Repository) as IBDbRepository;
            }
            else {
                EntityName = Type.Name;
            }
 

        }
    }
}