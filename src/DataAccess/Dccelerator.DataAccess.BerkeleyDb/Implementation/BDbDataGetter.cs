using Dccelerator.DataAccess.Implementation;

namespace Dccelerator.DataAccess.BerkeleyDb.Implementation {
    class BDbDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        

        public BDbDataGetter(IReadingRepository readingRepository, IBDbEntityInfo info) : base(readingRepository, info) {

        }
    }
}