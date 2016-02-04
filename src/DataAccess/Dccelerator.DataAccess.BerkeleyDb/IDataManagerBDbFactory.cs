namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IDataManagerBDbFactory : IDataManagerFactory {
        IBDbEntityInfo InfoAbout<TEntity>();


        IBDbRepository Repository();


        IBDbSchema Schema();
    }
}