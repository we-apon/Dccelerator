using System;
using System.Reflection;


namespace Dccelerator.DataAccess.Infrastructure {
    class EntityInfo
    {
        public Type EntityType;
        public string EntityName;

        public bool IsCollection;
        public string[] ColumnNames;
        //public string KeyIdName;
        public string ChildIdKey;
        public string TargetPath;
        public string OwnerReferenceName;
        public Type TargetCollectionType;

        /// <summary>
        /// Contains name and type of key id field of entity.
        /// </summary>
        public PropertyInfo KeyId;

        /// <summary>
        /// Contains pairs of names and types of all persisted fields
        /// </summary>
        public Tuple<string, Type>[] PersistedFields;

        public bool AllowCache;
        public TimeSpan CacheTimeout;
        public Type RealRepositoryType;
        public IInternalReadingRepository GlobalReadingRepository;

        public IDataAccessRepository RealRepository;

        public EntityInfo[] Children;
    }
}