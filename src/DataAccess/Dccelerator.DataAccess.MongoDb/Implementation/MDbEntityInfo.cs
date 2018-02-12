using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Implementation;


namespace Dccelerator.DataAccess.MongoDb.Implementation
{
    class MDbEntityInfo : BaseEntityInfo<IMDbRepository>, IMdbEntityInfo
    {
        public MDbEntityInfo(Type entityType) : base(entityType) { }
        public Dictionary<string, PropertyInfo> NavigationProperties { get; }
        public IEnumerable<IIncludeon> Inclusions { get; }
    }
}
