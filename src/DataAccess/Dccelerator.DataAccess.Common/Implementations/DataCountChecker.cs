using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations {
    class DataCountChecker<TEntity> : DataFilterBase<TEntity, long>, IDataCountChecker<TEntity> where TEntity : class {
        readonly IInternalReadingRepository _repository;
        readonly IEntityInfo _info;


        public DataCountChecker(IInternalReadingRepository repository, IEntityInfo info) {
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