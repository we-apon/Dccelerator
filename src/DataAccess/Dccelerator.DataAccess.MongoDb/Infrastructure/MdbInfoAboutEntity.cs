using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.MongoDb.Implementation;


namespace Dccelerator.DataAccess.MongoDb.Infrastructure
{
    class MdbInfoAboutEntity {
        static readonly ConcurrentDictionary<Type, MDbEntityInfo> _infoCache = new ConcurrentDictionary<Type, MDbEntityInfo>();

        public MDbEntityInfo Info { get; }


        public MdbInfoAboutEntity(Type entityType) {
            MDbEntityInfo info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = new MDbEntityInfo(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }
    }
}