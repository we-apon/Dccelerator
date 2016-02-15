namespace Dccelerator.DataAccess.Ado {
    public interface IDataManagerAdoFactory : IDataManagerFactory {

        IAdoEntityInfo AdoInfoAbout<TEntity>();


        IAdoNetRepository AdoNetRepository();
    }
}