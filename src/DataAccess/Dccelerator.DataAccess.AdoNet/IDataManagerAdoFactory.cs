using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado {
    public interface IDataManagerAdoFactory : IDataManagerFactory {

        IEntityInfo AdoInfoAbout<TEntity>();
    }
}