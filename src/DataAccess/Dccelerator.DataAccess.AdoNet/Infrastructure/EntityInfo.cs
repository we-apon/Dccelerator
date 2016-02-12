using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Attributes;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Infrastructure {
/*

    abstract class EntityInfo : IEntityInfo {
        public Type EntityType { get; }


#if NET40
        public Type TypeInfo { get; }
#else
        public TypeInfo TypeInfo { get; }
#endif

        public string EntityName => _entityName ?? (_entityName = string.IsNullOrWhiteSpace(EntityAttribute.Name) ? EntityType.Name : EntityAttribute.Name);
        string _entityName;


        public bool IsCollection => false;
        public string[] ColumnNames { get; set; }
        //public string KeyIdName => null;
        public string ChildIdKey => null;
        public string TargetPath => null;
        public string OwnerReferenceName => null;
        public Type TargetCollectionType => null;


        /// <summary>
        /// Contains name and type of key id field of entity.
        /// Can be null, because syntetic entities may hasn't unique identifier.
        /// </summary>
        public PropertyInfo KeyId => string.IsNullOrWhiteSpace(EntityAttribute.IdProperty)
            ? null
            : _keyId ?? (_keyId = EntityType.GetProperty(EntityAttribute.IdProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

        PropertyInfo _keyId;



        /// <summary>
        /// Contains pairs of names and types of all persisted fields
        /// </summary>
        public Tuple<string, Type>[] PersistedFields {
            get {
                if (_persistedFields != null)
                    return _persistedFields;

                _persistedFields = EntityType.GetProperties()
                    .Where(x => x.CanRead && !x.IsDefined<NotPersistedAttribute>()
                                && (x.PropertyType.IsAssignableFrom(_stringType) || _byteArrayType.IsAssignableFrom(x.PropertyType)
                                    || (!_enumerableType.IsAssignableFrom(x.PropertyType) && !x.PropertyType.GetInfo().IsClass)))
                    .Select(x => new Tuple<string, Type>(x.Name, x.PropertyType))
                    .ToArray();

                return _persistedFields;
            }
        }

        Tuple<string, Type>[] _persistedFields;



        public bool AllowCache => _allowCache ?? (_allowCache = EntityAttribute is GloballyCachedEntityAttribute).Value;
        bool? _allowCache;


        public TimeSpan CacheTimeout => _cacheTimeout ?? (_cacheTimeout = (EntityAttribute as GloballyCachedEntityAttribute)?.Timeout ?? TimeSpan.Zero).Value;
        TimeSpan? _cacheTimeout;


        public Type RealRepositoryType => EntityAttribute.Repository;


        public abstract IInternalReadingRepository GlobalReadingRepository { get; }


        public IAdoNetRepository RealRepository => _realRepository ?? (_realRepository = (IAdoNetRepository)Activator.CreateInstance(RealRepositoryType));
        IAdoNetRepository _realRepository;


        public IEntityInfo[] Children { get; }



        public EntityAttribute EntityAttribute { get; }




        public EntityInfo(Type type) {
            EntityType = type;
            TypeInfo = type.GetInfo();

            //EntityAttribute = TypeInfo.GetConfigurationForRepository()



            var inclusions = TypeInfo.GetCustomAttributes<IncludeChildrenAttribute>().OrderBy(x => x.ResultSetIndex).ToArray();
            if (inclusions.Length == 0)
                return;

            if (KeyId == null) {
                var possibleNames = new [] {
                    "Id",
                    EntityType.Name + "Id",
                    EntityType.Name + "_Id",
                    EntityName + "Id",
                    EntityName + "_Id"
                };

                _keyId = EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => possibleNames.Any(z => string.Compare(x.Name, z, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (_keyId == null)
                    throw new InvalidOperationException($"In order to use inclusions possibility, you must specify {nameof(EntityAttribute.IdProperty)} in {nameof(EntityAttribute)} " +
                                                            $"on entity {EntityType}, or use identifier in property named like: {string.Join(", ", possibleNames)}. Case of property name letters are ignored.");
            }



            Children = new IEntityInfo[inclusions.Length];
            for (var i = 0; i < inclusions.Length; i++) {
                Children[i] = new IncludeonInfo(this, inclusions[i]);
            }

        }

        


        static readonly Type _stringType = typeof(string);
        static readonly Type _byteArrayType = typeof(byte[]);
        static readonly Type _enumerableType = typeof(IEnumerable);

    }

*/

}