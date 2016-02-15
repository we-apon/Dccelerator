using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dccelerator.DataAccess.Ado.Infrastructure;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Ado.ReadingRepositories {
    public abstract class CachedReadingRepository : DirectReadingRepository
    {
        readonly ConcurrentDictionary<string, EntitiesCache> _entities = new ConcurrentDictionary<string, EntitiesCache>();
        


        class EntitiesCache {
            public object[] Entities;
            public DateTime QueriedTime;
        }


        protected virtual TimeSpan CacheTimeoutOf(IAdoEntityInfo info) {
            return info.CacheTimeout;
        }


        //todo: optimize 'SelectColumn', 'CountOf' and 'Any' methods

        #region Overrides of DirectReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public override IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var identityString = IdentityStringOf(info.EntityName, criteria);

            var timeout = CacheTimeoutOf((IAdoEntityInfo)info);
            if (timeout == TimeSpan.Zero)
                return base.Read(info, criteria);

            EntitiesCache cache;
            if (!_entities.TryGetValue(identityString, out cache) || (DateTime.UtcNow - cache.QueriedTime) > timeout) {
                cache = new EntitiesCache {
                    /*Entities = base.Read(info, criteria).ToArray(),*/
                    QueriedTime = DateTime.UtcNow
                };

                if (!_entities.TryAdd(identityString, cache))
                    cache = _entities[identityString];
            }

            if (cache.Entities == null) {
                lock (cache) {
                    if (cache.Entities == null)
                        cache.Entities = base.Read(info, criteria).ToArray();
                }
            }

            return cache.Entities;
            
        }

        


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        public override IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read(info, criteria).Select(x => {
                object value;
                x.TryGetValueOnPath(columnName, out value);
                return value;
            });
        }



        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public override int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read(info, criteria).Count();
        }


        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public override bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read(info, criteria).Any();
        }

        #endregion
    }
}