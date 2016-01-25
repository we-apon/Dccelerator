using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations {
    class DataCountChecker<TEntity> : DataFilterBase<TEntity, long>, IDataCountChecker<TEntity> where TEntity : class {
        readonly IInternalReadingRepository _repository;
        readonly string _entityName;


        public DataCountChecker(IInternalReadingRepository repository, string entityName) {
            _repository = repository;
            _entityName = entityName;
        }


        #region Overrides of DataFilterBase<TEntity,long>

        protected override long ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.CountOf(_entityName, EntityType, criteria);
        }

        #endregion
    }
}