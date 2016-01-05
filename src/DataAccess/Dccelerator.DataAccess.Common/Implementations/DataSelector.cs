using System.Collections.Generic;
using System.Linq;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Implementations {
    class DataSelector<TEntity, TResult> : DataFilterBase<TEntity, IEnumerable<TResult>>, IDataSelector<TEntity, TResult> where TEntity : class, new() {
        private readonly IInternalReadingRepository _repository;
        private readonly string _selectedColumn;

        public DataSelector(IInternalReadingRepository repository, string selectedColumn) {
            _repository = repository;
            _selectedColumn = selectedColumn;
        }



        #region Overrides of DataFilterBase<TEntity,IEnumerable<TResult>>

        protected override IEnumerable<TResult> ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return _repository.ReadColumn(_selectedColumn, Info.EntityName, EntityType, criteria).Cast<TResult>();
        }

        #endregion


    }
}