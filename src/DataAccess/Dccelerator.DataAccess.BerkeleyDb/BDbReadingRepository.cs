using System;
using System.Collections.Generic;
using System.Linq;
using BerkeleyDB;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbReadingRepository : IInternalReadingRepository {
        readonly IBDbEntityInfo _info;


        public BDbReadingRepository(IBDbEntityInfo info) {
            _info = info;
        }

        
        IEnumerable<DatabaseEntry> GetEntriesFor(string entityName, ICollection<IDataCriterion> criteria) {
            var repository = _info.Repository;

            if (criteria.Count == 0)
                return repository.ContinuouslyReadToEnd(entityName);

            if (criteria.Count == 1) {
                var criterion = criteria.First();
                var entry = repository.EntryFrom(criterion);

                if (repository.IsPrimaryKey(criterion))
                    return repository.GetByKeyFromPrimaryDb(entry, entityName);


                DuplicatesPolicy policy;
                string indexSubName;

                ForeignKeyAttribute foreignKeyInfo;
                if (_info.ForeignKeys.TryGetValue(criterion.Name, out foreignKeyInfo)) {
                    policy = foreignKeyInfo.DuplicatesPolicy;
                    indexSubName = foreignKeyInfo.ForeignEntityNavigationPath;
                }
                else {
                    policy = DuplicatesPolicy.UNSORTED;
                    indexSubName = criterion.Name;
                }
                
                
                return repository.GetFromSecondaryDb(entry, entityName, indexSubName, policy);
            }

            return repository.GetByJoin(entityName, criteria);
        }

        

        #region Implementation of IInternalReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public IEnumerable<object> Read(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(entityName, criteria).Select(x => x.Data.FromBytes());
        }



        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public bool Any(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(entityName, criteria).Any();
        }


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        public IEnumerable<object> ReadColumn(string columnName, string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public int CountOf(string entityName, Type entityType, ICollection<IDataCriterion> criteria) {
            return GetEntriesFor(entityName, criteria).Count();
        }

        #endregion
    }
}