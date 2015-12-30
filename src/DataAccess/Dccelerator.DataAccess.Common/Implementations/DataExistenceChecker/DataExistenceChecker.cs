using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations.DataExistenceChecker {
    class DataExistenceChecker<TEntity> : DataFilterBase<TEntity, bool>, IDataExistenceChecker<TEntity> where TEntity : class {
        private readonly IInternalReadingRepository _repository;


        public DataExistenceChecker(IInternalReadingRepository repository) {
            _repository = repository;
        }


        #region Overrides of DataFilterBase<TEntity,bool>

        protected override bool ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.Any(Info.EntityName, EntityType, criteria);
        }

        #endregion
    }



}