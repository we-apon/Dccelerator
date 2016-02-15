using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Ado.BasicImplementation.Internal {

    class AdoNetInfoAbout<TRepository, TEntity> where TRepository : class, IAdoNetRepository {
        static readonly AdoNetInfoAboutEntity<TRepository> _infoContainer = new AdoNetInfoAboutEntity<TRepository>(RUtils<TEntity>.Type);

        public static AdoEntityInfo<TRepository> Info => _infoContainer.Info;

    }


    class AdoNetInfoAboutEntity<TRepository> where TRepository : class, IAdoNetRepository {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        static readonly ConcurrentDictionary<Type, AdoEntityInfo<TRepository>> _infoCache = new ConcurrentDictionary<Type, AdoEntityInfo<TRepository>>();


        public AdoEntityInfo<TRepository> Info { get; }


        public AdoNetInfoAboutEntity(Type entityType) {
            AdoEntityInfo<TRepository> info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = new AdoEntityInfo<TRepository>(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }
    }
}