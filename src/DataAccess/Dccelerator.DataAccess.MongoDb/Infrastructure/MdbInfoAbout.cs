using Dccelerator.DataAccess.MongoDb.Implementation;
using Dccelerator.UnFastReflection;


namespace Dccelerator.DataAccess.MongoDb.Infrastructure
{
    class MdbInfoAbout<TEntity>
    {
        static readonly MdbInfoAboutEntity _infoContainer = new MdbInfoAboutEntity(RUtils<TEntity>.Type);

        public static MDbEntityInfo Info => _infoContainer.Info;
    }
}