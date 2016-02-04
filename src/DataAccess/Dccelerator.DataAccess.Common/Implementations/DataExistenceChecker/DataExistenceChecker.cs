using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations.DataExistenceChecker {
    class DataExistenceChecker<TEntity> : DataFilterBase<TEntity, bool>, IDataExistenceChecker<TEntity> where TEntity : class {
        readonly IInternalReadingRepository _repository;
        readonly IEntityInfo _info;


        public DataExistenceChecker(IInternalReadingRepository repository, IEntityInfo info) {
            _repository = repository;
            _info = info;
        }


        #region Overrides of DataFilterBase<TEntity,bool>

        protected override bool ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.Any(_info, criteria);
        }

        #endregion
    }



}