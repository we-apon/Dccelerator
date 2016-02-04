using Dccelerator.DataAccess.Implementations.DataGetters;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        

        public BDbDataGetter(IInternalReadingRepository readingRepository, IBDbEntityInfo info) : base(readingRepository, info) {

        }
    }
}