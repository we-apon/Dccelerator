using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Ado.Infrastructure;
using Dccelerator.Reflection;

#if !NET40
using System.Reflection;
#endif

namespace Dccelerator.DataAccess.Ado {

    class AdoEntityInfo : BaseEntityInfo<IAdoNetRepository>, IAdoEntityInfo {

        readonly object _lock = new object();


        public AdoEntityInfo(Type entityType) : base(entityType) { }



        #region Implementation of IAdoEntityInfo

        public Dictionary<string, SecondaryKeyAttribute> SecondaryKeys { get; }

        public Dictionary<string, PropertyInfo> PersistedProperties {
            get {
                if (_persistedProperties == null) {
                    lock (_lock) {
                        if (_persistedProperties == null)
                            _persistedProperties = EntityType.Properties(BindingFlags.Instance | BindingFlags.Public, IsPersistedProperty);
                    }
                }
                return _persistedProperties;
            }
        }
        Dictionary<string, PropertyInfo> _persistedProperties;



        public Dictionary<string, PropertyInfo> NavigationProperties { get; }


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
        static readonly Type _notPersistedAttributeType = typeof(NotPersistedAttribute);


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

        

        static readonly Type _stringType = typeof(string);
        static readonly Type _byteArrayType = typeof(byte[]);
        static readonly Type _enumerableType = typeof(IEnumerable);


        protected virtual Func<PropertyInfo, bool> IsPersistedProperty { get; } = _isPersistedPropertyDefaultImplementation;


        static readonly Func<PropertyInfo, bool> _isPersistedPropertyDefaultImplementation = property => {

            //? if marked with NotPersistedAttribute
            if (property.GetCustomAttributesData().Any(x => {
#if NET40
                return x.Constructor.DeclaringType == _notPersistedAttributeType;
#else
                return x.AttributeType == _notPersistedAttributeType;
#endif
            }))
                return false;


            if (!property.CanRead)
                return false;

            var type = property.PropertyType;

            if (type == _stringType || type.IsAssignableFrom(_byteArrayType))
                return true;

            if (_enumerableType.IsAssignableFrom(type) || type.IsClass)
                return false;

            return true;
        };




        Dictionary<int, Includeon> GetInclusions() {
            var inclusions = new Dictionary<int, Includeon>();

            var inclusionAttributes = TypeInfo.GetCustomAttributes<IncludeChildrenAttribute>().ToArray();
            if (inclusionAttributes.Length == 0)
                return inclusions;

            foreach (var inclusionAttribute in inclusionAttributes) {
                var includeon = new Includeon(inclusionAttribute, this);
                inclusions.Add(inclusionAttribute.ResultSetIndex, includeon);
            }

            return inclusions;
        }

        #endregion
    }


}