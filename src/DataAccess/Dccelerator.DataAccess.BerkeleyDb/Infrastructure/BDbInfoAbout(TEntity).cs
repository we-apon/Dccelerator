using Dccelerator.DataAccess.BerkeleyDb.Implementation;
using Dccelerator.Reflection;

namespace Dccelerator.DataAccess.BerkeleyDb.Infrastructure {
    class BDbInfoAbout<TEntity> {
        static readonly BdbInfoAboutEntity _infoContainer = new BdbInfoAboutEntity(RUtils<TEntity>.Type);

        public static BDbEntityInfo Info => _infoContainer.Info;
    }
}