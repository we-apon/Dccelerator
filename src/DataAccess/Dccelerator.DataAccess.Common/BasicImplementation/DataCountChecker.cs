using System.Collections.Generic;


namespace Dccelerator.DataAccess.BasicImplementation {
    class DataCountChecker<TEntity> : DataFilterBase<TEntity, long>, IDataCountChecker<TEntity> where TEntity : class {
        readonly IReadingRepository _repository;
        readonly IEntityInfo _info;


        public DataCountChecker(IReadingRepository repository, IEntityInfo info) {
            _repository = repository;
            _info = info;
        }


        #region Overrides of DataFilterBase<TEntity,long>

        protected override long ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.CountOf(_info, criteria);
        }

        #endregion
    }
}