using Dccelerator.DataAccess.Implementations.DataGetters;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        

        public BDbDataGetter(string entityName, string environmentPath, string dbFilePath, string password) : base(new BDbReadingRepository(environmentPath, dbFilePath, password), entityName) {

        }
    }
}