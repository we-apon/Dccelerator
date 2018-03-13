using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Implementation;
using Dccelerator.Reflection;

namespace Dccelerator.DataAccess.Ado.Implementation {

    public abstract class AdoEntityInfo<TRepository, TDbTypeEnum> : BaseEntityInfo<TRepository>, IAdoEntityInfo<TDbTypeEnum> 
        where TRepository : class, IAdoNetRepository
        where TDbTypeEnum : struct 
    {
        
        protected abstract IAdoEntityInfo GetInstance(Type type);
        public abstract TDbTypeEnum GetDefaultDbType(Type propertyType);


        readonly object _lock = new object();

        protected AdoEntityInfo(Type entityType) : base(entityType) { }


        void IAdoEntityInfo.SetupRepository(IAdoNetRepository repository) {
            var genericRepository = repository as TRepository;
            if (genericRepository == null)
                throw new InvalidOperationException($"{nameof(repository)} should be of type {typeof(TRepository)}.");

            if (Repository != null)
                throw new InvalidOperationException("Repository already initialized.");

            Repository = genericRepository;
        }




        private Dictionary<string, TDbTypeEnum> _typeMappings;

        protected virtual Dictionary<string, TDbTypeEnum> TypeMappings {
            get {
                if (_typeMappings != null)
                    return _typeMappings;

                return _typeMappings = GetDbTypeMappings();
            }
        }



        public virtual TDbTypeEnum GetParameterDbType(IDataCriterion criterion) =>
            TypeMappings.Value(criterion.Name)
                .Or(() => GetDefaultDbType(criterion.Type ?? criterion.Value?.GetType()));


        #region Implementation of IAdoEntityInfo

        public virtual Dictionary<string, PropertyInfo> NavigationProperties { get; }

        /// <summary>
        /// Cached insert sql queries. Key is FullName of repository type.
        /// </summary>
        public ConcurrentDictionary<string, string> CachedInsertQueries { get; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Cached update sql queries. Key is FullName of repository type.
        /// </summary>
        public ConcurrentDictionary<string, string> CachedUpdateQueries { get; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Cached delete sql queries. Key is FullName of repository type.
        /// </summary>
        public ConcurrentDictionary<string, string> CachedDeleteQueries { get; } = new ConcurrentDictionary<string, string>();


        IAdoNetRepository IAdoEntityInfo.Repository => Repository;

        public virtual string[] ReaderColumns { get; protected set; }



        protected virtual Dictionary<string, TDbTypeEnum> GetDbTypeMappings() {
            var mappings = new Dictionary<string, TDbTypeEnum>();
            foreach (var property in PersistedProperties.Values) {
                TDbTypeEnum result;

                var dbTypeAttribute = property.GetMany<DbTypeAttribute>().MostApplicableToRepository(Repository?.GetType() ?? typeof(TRepository));

                if (dbTypeAttribute != null) {
                    if (dbTypeAttribute.DbTypeName is TDbTypeEnum)
                        result = (TDbTypeEnum) dbTypeAttribute.DbTypeName;
                    else if (!Enum.TryParse(dbTypeAttribute.DbTypeName.ToString(), out result))
                        result = GetDefaultDbType(property.PropertyType);
                }
                else {
                    result = GetDefaultDbType(property.PropertyType);
                }

                mappings.Add(property.Name, result);
            }


            return mappings;
        }


        Dictionary<string, int> _readerColumnsIndexes;
        public int IndexOf(string columnName) {
            if (_readerColumnsIndexes == null) {
                lock (_lock) {
                    if (_readerColumnsIndexes == null) {
                        _readerColumnsIndexes = new Dictionary<string, int>(ReaderColumns.Length);

                        for (var i = 0; i < ReaderColumns.Length; i++) {
                            _readerColumnsIndexes.Add(ReaderColumns[i], i);
                        }
                    }
                }
            }

            return _readerColumnsIndexes[columnName];
        }


        public void InitReaderColumns(DbDataReader reader) {
            if (ReaderColumns != null)
                return;
            
            var columns = reader.GetSchemaTable()?.Rows.Cast<DataRow>().Select(x => (string) x[0]).ToArray();

            lock (_lock) {
                if (ReaderColumns == null)
                    ReaderColumns = columns;
            }
        }




        Dictionary<int, Includeon> _inclusions;


        public Dictionary<int, Includeon> Inclusions {
            get {
                if (_inclusions == null) {
                    lock (_lock) {
                        if (_inclusions == null)
                            _inclusions = GetInclusions();
                    }
                }

                return _inclusions;
            }
        }


        IEnumerable<IIncludeon> IEntityInfo.Inclusions => Inclusions.Values.Any() ? Inclusions.Values : null;

        





        Dictionary<int, Includeon> GetInclusions() {
            var inclusions = new Dictionary<int, Includeon>();

            var inclusionAttributes = TypeInfo.GetCustomAttributes<IncludeChildrenAttribute>().ToArray();
            if (inclusionAttributes.Length == 0)
                return inclusions;

            foreach (var inclusionAttribute in inclusionAttributes) {
                var includeon = new Includeon(inclusionAttribute, this, GetInstance);
                inclusions.Add(inclusionAttribute.ResultSetIndex, includeon);
            }

            return inclusions;
        }
        


        #endregion
    }


}