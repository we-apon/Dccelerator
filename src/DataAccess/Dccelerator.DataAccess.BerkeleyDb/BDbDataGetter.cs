using Dccelerator.DataAccess.Implementations.DataGetters;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        

        public BDbDataGetter(IBDbEntityInfo info) : base(new BDbReadingRepository(info), info.EntityName) {

        }
    }
}