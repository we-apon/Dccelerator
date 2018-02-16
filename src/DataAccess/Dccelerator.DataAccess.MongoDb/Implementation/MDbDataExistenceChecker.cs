using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Implementation;


namespace Dccelerator.DataAccess.MongoDb.Implementation {
    class MDbDataExistenceChecker<TEntity> : DataFilterBase<TEntity, bool>, IDataExistenceChecker<TEntity> where TEntity : class {
        IReadingRepository readingRepository;
        IEntityInfo entityInfo;


        public MDbDataExistenceChecker(IReadingRepository readingRepository, IEntityInfo entityInfo)
        {
            this.readingRepository = readingRepository;
            this.entityInfo = entityInfo;
        }

        protected override bool ApplyFilterWith(ICollection<IDataCriterion> criteria) {
            return readingRepository.Any(entityInfo, criteria);
        }
    }
}
