using Dccelerator.DataAccess.Infrastructure;
using System;
using System.Collections.Concurrent;
using Dccelerator.DataAccess.Ado.ReadingRepositories;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Ado {

    class ConfigurationOf<TEntity> where TEntity : class {

        // ReSharper disable StaticMemberInGenericType
        static readonly ConfigurationOfEntity _configuration = new ConfigurationOfEntity(RUtils<TEntity>.Type);

        // ReSharper restore StaticMemberInGenericType

        public static EntityInfo Info => _configuration.Info;


        public static IInternalReadingRepository GetReadingRepository() {
            return _configuration.Info.AllowCache
                ? new CachedReadingRepository()
                : new DirectReadingRepository();
        }
    }


    class ConfigurationOfEntity {
        static readonly ConcurrentDictionary<Type, EntityInfo> _infoCache = new ConcurrentDictionary<Type, EntityInfo>();

        public EntityInfo Info { get; }



        public ConfigurationOfEntity(Type entityType) {
            EntityInfo info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = new AdoNetEntityInfo(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }

    }
}