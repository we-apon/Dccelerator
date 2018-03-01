using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Common.Implementation;

namespace Dccelerator.DataAccess.MongoDb.Implementation
{
    public class MDbCachedReadingRepository : MDbReadingRepository
    {
        readonly ConcurrentDictionary<string, EntitiesCache> _entities = new ConcurrentDictionary<string, EntitiesCache>();
        static readonly ConcurrentDictionary<string, EntitiesCache> _globallyCachedEntities = new ConcurrentDictionary<string, EntitiesCache>();

        public override IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }
    }
}
