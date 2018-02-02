using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;


namespace Dccelerator.DataAccess.MongoDb.Implementation
{
    public class MDbReadingRepository : IReadingRepository
    {
        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        public bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        public IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        public int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }
    }
}
