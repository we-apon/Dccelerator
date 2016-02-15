using System;
using System.Collections.Generic;
using System.Linq;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb.Implementation {
    class BDbReadingRepository : IReadingRepository {

        public static IReadingRepository Instance =>  _readingRepository ?? (_readingRepository = new BDbReadingRepository());
        static IReadingRepository _readingRepository;


        BDbReadingRepository() { }



        IEnumerable<DatabaseEntry> GetEntriesFor(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var dbdInfo = (IBDbEntityInfo) info;

            var repository = dbdInfo.Repository;

            if (criteria.Count == 0)
                return repository.ContinuouslyReadToEnd(info.EntityName);

            
            if (criteria.Count == 1) {
                var criterion = criteria.First();
                var entry = repository.EntryFrom(criterion);

                if (repository.IsPrimaryKey(criterion))
                    return repository.GetByKeyFromPrimaryDb(entry, info.EntityName);


                DuplicatesPolicy policy;
                string indexSubName;

                ForeignKeyAttribute foreignKeyInfo;
                if (info.ForeignKeys.TryGetValue(criterion.Name, out foreignKeyInfo)) {
                    policy = foreignKeyInfo.DuplicatesPolicy;
                    indexSubName = foreignKeyInfo.NavigationPropertyPath;
                }
                else {
                    policy = DuplicatesPolicy.UNSORTED;
                    indexSubName = criterion.Name;
                }
                
                
                return repository.GetFromSecondaryDb(entry, info.EntityName, indexSubName, policy);
            }

            return repository.GetByJoin(dbdInfo, criteria);
        }

        

        #region Implementation of IInternalReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(info, criteria).Select(x => x.Data.FromBytes());
        }



        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(info, criteria).Any();
        }


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        public IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(info, criteria).Count();
        }

        #endregion
    }
}