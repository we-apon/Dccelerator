using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Dccelerator.Reflection;

namespace Dccelerator.DataAccess.Ado.Implementation.Internal {


    class AdoNetInfoCache<TEntityInfo, TEntity> where TEntityInfo : IAdoEntityInfo {
        static readonly AdoNetInfoCacheContainer<TEntityInfo> _infoContainer = new AdoNetInfoCacheContainer<TEntityInfo>(RUtils<TEntity>.Type, type => (TEntityInfo) Activator.CreateInstance(typeof(TEntityInfo), type));

        public static TEntityInfo Info => _infoContainer.Info;
    }


    class AdoNetInfoCacheContainer<TEntityInfo> where TEntityInfo : IAdoEntityInfo {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        static readonly ConcurrentDictionary<Type, TEntityInfo> _infoCache = new ConcurrentDictionary<Type, TEntityInfo>();


        public TEntityInfo Info { get; }


        public AdoNetInfoCacheContainer(Type entityType, Func<Type, TEntityInfo> getInstance) {
            TEntityInfo info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = getInstance(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }
    }

}