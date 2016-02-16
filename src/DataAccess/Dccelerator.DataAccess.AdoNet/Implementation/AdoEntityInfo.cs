using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Implementation;
using Dccelerator.Reflection;

#if !NET40
using System.Reflection;
#endif

namespace Dccelerator.DataAccess.Ado.Implementation {

    public class AdoEntityInfo<TRepository> : BaseEntityInfo<TRepository>, IAdoEntityInfo where TRepository : class, IAdoNetRepository {

        readonly object _lock = new object();


        public AdoEntityInfo(Type entityType) : base(entityType) { }


        internal void SetRepository(IAdoNetRepository repository) {
            var genericRepository = repository as TRepository;
            if (genericRepository == null)
                throw new InvalidOperationException($"{nameof(repository)} should be of type {typeof(TRepository)}.");

            if (Repository != null)
                throw new InvalidOperationException("Repository already initialized.");

            Repository = genericRepository;
        }


        #region Implementation of IAdoEntityInfo

        public Dictionary<string, PropertyInfo> NavigationProperties { get; }


        IAdoNetRepository IAdoEntityInfo.Repository => Repository;

        public string[] ReaderColumns { get; private set; }


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

#if NET40 || NET45
            var columns = reader.GetSchemaTable()?.Rows.Cast<DataRow>().Select(x => (string) x[0]).ToArray();
#else
            throw new NotImplementedException() and don't build until it's implemented!
#endif
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
                var includeon = new Includeon(inclusionAttribute, this, type => new AdoEntityInfo<TRepository>(type));
                inclusions.Add(inclusionAttribute.ResultSetIndex, includeon);
            }

            return inclusions;
        }

        #endregion
    }


}