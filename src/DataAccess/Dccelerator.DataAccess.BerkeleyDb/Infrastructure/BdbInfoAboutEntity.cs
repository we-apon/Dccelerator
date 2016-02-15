using System;
using System.Collections.Concurrent;
using Dccelerator.DataAccess.BerkeleyDb.Implementation;


namespace Dccelerator.DataAccess.BerkeleyDb.Infrastructure {
    class BdbInfoAboutEntity {
        static readonly ConcurrentDictionary<Type, BDbEntityInfo> _infoCache = new ConcurrentDictionary<Type, BDbEntityInfo>();


        public BDbEntityInfo Info { get; }


        public BdbInfoAboutEntity(Type entityType) {
            BDbEntityInfo info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = new BDbEntityInfo(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }
    }
}