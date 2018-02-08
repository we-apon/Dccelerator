using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dccelerator.DataAccess.MongoDb {
    public interface IMdbEntityInfo : IEntityInfo {
        IMDbRepository Repository { get; }
    }
}