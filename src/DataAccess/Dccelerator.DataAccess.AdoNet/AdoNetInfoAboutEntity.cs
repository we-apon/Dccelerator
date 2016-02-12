using System;
using System.Collections.Concurrent;
using System.Reflection;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Ado {

    class AdoNetInfoAbout<TEntity> {
        static readonly AdoNetInfoAboutEntity _infoContainer = new AdoNetInfoAboutEntity(RUtils<TEntity>.Type);

        public static AdoEntityInfo Info => _infoContainer.Info;

    }


    class AdoNetInfoAboutEntity {
        static readonly ConcurrentDictionary<Type, AdoEntityInfo> _infoCache = new ConcurrentDictionary<Type, AdoEntityInfo>();


        public AdoEntityInfo Info { get; }


        public AdoNetInfoAboutEntity(Type entityType) {
            AdoEntityInfo info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = new AdoEntityInfo(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }
    }
}