using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Attributes;
using Dccelerator.DataAccess.Implementations.ReadingRepositories;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Infrastructure {
    class EntityInfo : IEntityInfo {
        public Type Type { get; }


#if NET40
        public Type TypeInfo { get; }
#else
        public TypeInfo TypeInfo { get; }
#endif

        public string EntityName => _entityName ?? (_entityName = string.IsNullOrWhiteSpace(EntityAttribute.Name) ? Type.Name : EntityAttribute.Name);
        string _entityName;


        public bool IsCollection => false;
        public string[] ColumnNames => null;
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
            : _keyId ?? (_keyId = Type.GetProperty(EntityAttribute.IdProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

        PropertyInfo _keyId;



        /// <summary>
        /// Contains pairs of names and types of all persisted fields
        /// </summary>
        public Tuple<string, Type>[] PersistedFields {
            get {
                if (_persistedFields != null)
                    return _persistedFields;

                _persistedFields = Type.GetProperties()
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


        public IInternalReadingRepository GlobalReadingRepository => _readingRepository ?? (_readingRepository = AllowCache ? new CachedReadingRepository() : new DirectReadingRepository());
        IInternalReadingRepository _readingRepository;


        public IDataAccessRepository RealRepository => _realRepository ?? (_realRepository = (IDataAccessRepository)Activator.CreateInstance(RealRepositoryType));
        IDataAccessRepository _realRepository;


        public IEntityInfo[] Children;



        public EntityAttribute EntityAttribute { get; }




        public EntityInfo(Type type) {
            Type = type;
            TypeInfo = type.GetInfo();

            EntityAttribute = TypeInfo.GetEntityAttribute();



            var inclusions = TypeInfo.GetCustomAttributes<IncludeChildrenAttribute>().OrderBy(x => x.ResultSetIndex).ToArray();
            if (inclusions.Length == 0)
                return;

            if (KeyId == null) {
                var possibleNames = new [] {
                    "Id",
                    Type.Name + "Id",
                    Type.Name + "_Id",
                    EntityName + "Id",
                    EntityName + "_Id"
                };

                _keyId = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => possibleNames.Any(z => string.Compare(x.Name, z, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (_keyId == null)
                    throw new InvalidOperationException($"In order to use inclusions possibility, you must specify {nameof(EntityAttribute.IdProperty)} in {nameof(EntityAttribute)} " +
                                                            $"on entity {Type}, or use identifier in property named like: {string.Join(", ", possibleNames)}. Case of property name letters are ignored.");
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
}