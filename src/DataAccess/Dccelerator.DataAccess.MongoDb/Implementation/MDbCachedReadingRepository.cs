using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Common.Implementation;


namespace Dccelerator.DataAccess.MongoDb.Implementation {
    public class MDbCachedReadingRepository : MDbReadingRepository {
        readonly ConcurrentDictionary<string, EntitiesCache> _entities = new ConcurrentDictionary<string, EntitiesCache>();
        static readonly ConcurrentDictionary<string, EntitiesCache> _globallyCachedEntities = new ConcurrentDictionary<string, EntitiesCache>();


        public override IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var identityString = IdentityStringOf(info.EntityName, criteria);

            var timeout = info.CacheTimeout;
            if (timeout == TimeSpan.Zero)
                return base.Read(info, criteria);

            if (info.IsGloballyCached)
                return GetFromCache(_globallyCachedEntities, info, identityString, criteria, timeout);


            return GetFromCache(_entities, info, identityString, criteria, timeout);
        }


        IEnumerable<object> GetFromCache(ConcurrentDictionary<string, EntitiesCache> entities, IEntityInfo info, string identityString, ICollection<IDataCriterion> criteria, TimeSpan timeout) {
            EntitiesCache cache;
            if (!entities.TryGetValue(identityString, out cache) || (DateTime.UtcNow - cache.QueriedTime) > timeout) {
                cache = new EntitiesCache {
                    QueriedTime = DateTime.UtcNow
                };

                if (!entities.TryAdd(identityString, cache))
                    cache = entities[identityString];
            }

            if (cache.Entities == null) {
                lock (cache) {
                    if (cache.Entities == null)
                        cache.Entities = base.Read(info, criteria).ToArray();
                }
            }

            return cache.Entities;
        }


        protected string IdentityStringOf(string entityName, IEnumerable<IDataCriterion> criteria) {
            var builder = new StringBuilder(entityName);
            foreach (var criterion in criteria) {
                builder.Append(criterion.Name).Append(criterion.Value);
            }
            return builder.ToString();
        }


        public override bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read(info, criteria).Any();
        }


        public override IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        public override int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read(info, criteria).Count();
        }
    }
}
