using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Implementations.ReadingRepositories {
    internal class CachedReadingRepository : DirectReadingRepository
    {
        readonly ConcurrentDictionary<string, EntitiesCache> _entities = new ConcurrentDictionary<string, EntitiesCache>();
        
        class EntitiesCache {
            public object[] Entities;
            public DateTime QueriedTime;
        }

        protected virtual TimeSpan CacheTimeoutOf(Type entityType) {
            return ConfigOf(entityType).Info.CacheTimeout;
        }
        
        //todo: optimize 'SelectColumn', 'CountOf' and 'Any' methods

        #region Overrides of DirectReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public override IEnumerable<object> Read(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            var identityString = IdentityStringOf(entityName, criteria);

            var timeout = CacheTimeoutOf(entityType);
            if (timeout == TimeSpan.Zero)
                return base.Read(entityName, entityType, criteria);

            EntitiesCache cache;
            if (!_entities.TryGetValue(identityString, out cache) || (DateTime.UtcNow - cache.QueriedTime) > timeout) {
                cache = new EntitiesCache {
                    Entities = base.Read(entityName, entityType, criteria).ToArray(),
                    QueriedTime = DateTime.UtcNow
                };

                if (!_entities.TryAdd(identityString, cache))
                    cache = _entities[identityString];
            }

            return cache.Entities;
        }

        


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        public override IEnumerable<object> ReadColumn(string columnName, string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return Read(entityName, entityType, criteria).Select(x => {
                object value;
                TypeManipulator.TryGetNestedProperty(x, columnName, out value);
                return value;
            });
        }



        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public override int CountOf(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return Read(entityName, entityType, criteria).Count();
        }


        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public override bool Any(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return Read(entityName, entityType, criteria).Any();
        }

        #endregion
    }
}