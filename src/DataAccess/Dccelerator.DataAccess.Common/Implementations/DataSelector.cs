using System.Collections.Generic;
using System.Linq;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations {
    class DataSelector<TEntity, TResult> : DataFilterBase<TEntity, IEnumerable<TResult>>, IDataSelector<TEntity, TResult> where TEntity : class, new() {
        readonly IInternalReadingRepository _repository;
        readonly IEntityInfo _info;
        readonly string _selectedColumn;

        public DataSelector(IInternalReadingRepository repository, IEntityInfo info, string selectedColumn) {
            _repository = repository;
            _info = info;
            _selectedColumn = selectedColumn;
        }



        #region Overrides of DataFilterBase<TEntity,IEnumerable<TResult>>

        protected override IEnumerable<TResult> ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.ReadColumn(_selectedColumn, _info, criteria).Cast<TResult>();
        }

        #endregion


    }
}