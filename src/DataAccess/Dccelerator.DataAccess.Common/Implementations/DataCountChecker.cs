using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations {
    class DataCountChecker<TEntity> : DataFilterBase<TEntity, long>, IDataCountChecker<TEntity> where TEntity : class {
        private readonly IInternalReadingRepository _repository;


        public DataCountChecker(IInternalReadingRepository repository) {
            _repository = repository;
        }


        #region Overrides of DataFilterBase<TEntity,long>

        protected override long ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.CountOf(Info.EntityName, EntityType, criteria);
        }

        #endregion
    }
}