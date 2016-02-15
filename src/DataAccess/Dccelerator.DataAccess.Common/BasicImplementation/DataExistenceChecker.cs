using System.Collections.Generic;


namespace Dccelerator.DataAccess.BasicImplementation {
    class DataExistenceChecker<TEntity> : DataFilterBase<TEntity, bool>, IDataExistenceChecker<TEntity> where TEntity : class {
        readonly IReadingRepository _repository;
        readonly IEntityInfo _info;


        public DataExistenceChecker(IReadingRepository repository, IEntityInfo info) {
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