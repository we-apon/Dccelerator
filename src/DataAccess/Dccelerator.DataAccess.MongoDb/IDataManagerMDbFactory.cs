using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dccelerator.DataAccess.MongoDb
{
    public interface IDataManagerMDbFactory : IDataManagerFactory {

        IMdbEntityInfo MDbInfoAbout<TEntity>();

        IMDbRepository Repository();
    }
}
