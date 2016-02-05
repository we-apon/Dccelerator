namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IDataManagerBDbFactory : IDataManagerFactory {
        IBDbEntityInfo BerkeleyInfoAbout<TEntity>();


        IBDbRepository Repository();


        IBDbSchema Schema();
    }
}