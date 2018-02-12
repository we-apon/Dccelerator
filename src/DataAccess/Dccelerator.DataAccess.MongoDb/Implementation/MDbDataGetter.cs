using Dccelerator.DataAccess.Implementation;


namespace Dccelerator.DataAccess.MongoDb.Implementation {
    class MDbDataGetter<TEntity> : DataGetterBase<TEntity> where TEntity : class, new() {
        public MDbDataGetter(IReadingRepository repository, IEntityInfo info) : base(repository, info) { }
    }
}