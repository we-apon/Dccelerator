using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess
{
    public class Get
    {
        #region static
        
        private static readonly ConcurrentDictionary<Type, EntityInfo> _infos = new ConcurrentDictionary<Type, EntityInfo>();


        public static Get Entity(Type type) {
            return new Get(type);
        }
        
        #endregion



        private readonly Type _entityType;
        internal readonly EntityInfo _info;
        internal IInternalReadingRepository _customRepository;
        private string _customEntityName;

        
        public EntityAttribute EntityAttribute;


        protected Get(Type entityType) {
            _entityType = entityType;
            EntityInfo info;
            if (!_infos.TryGetValue(entityType, out info)) {
                info = new ConfigurationOfEntity(entityType).Info;
                if (!_infos.TryAdd(entityType, info))
                    info = _infos[entityType];
            }

            _info = info;
        }


        internal Get From(IInternalReadingRepository repository) {
            _customRepository = repository;
            return this;
        }

        public Get Using(string customEntityName) {
            _customEntityName = customEntityName;
            return this;
        }

        
        public virtual IEnumerable<object> By(params IDataCriterion[] criteria) {
            var name = _customEntityName ?? _info.EntityName;
            var repository = _customRepository ?? _info.GlobalReadingRepository;
            return repository.Read(name, _entityType, criteria);
        }


        internal bool Any(ICollection<IDataCriterion> criteria) {
            var name = _customEntityName ?? _info.EntityName;
            var repository = _customRepository ?? _info.GlobalReadingRepository;
            return repository.Any(name, _entityType, criteria);
        }

        

    }
}