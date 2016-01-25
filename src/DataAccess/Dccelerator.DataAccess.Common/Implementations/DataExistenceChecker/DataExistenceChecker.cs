using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations.DataExistenceChecker {
    class DataExistenceChecker<TEntity> : DataFilterBase<TEntity, bool>, IDataExistenceChecker<TEntity> where TEntity : class {
        readonly IInternalReadingRepository _repository;
        readonly string _entityName;


        public DataExistenceChecker(IInternalReadingRepository repository, string entityName) {
            _repository = repository;
            _entityName = entityName;
        }


        #region Overrides of DataFilterBase<TEntity,bool>

        protected override bool ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.Any(_entityName, EntityType, criteria);
        }

        #endregion
    }



}