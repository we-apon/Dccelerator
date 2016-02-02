using System;
using System.Collections.Concurrent;


namespace Dccelerator.DataAccess.BerkeleyDb {
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