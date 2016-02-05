using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado {

    class AdoEntityInfo : IAdoEntityInfo {

        readonly object _lock = new object();


        public AdoEntityInfo(Type entityType) {
            EntityType = entityType;
        }

        #region Implementation of IEntityInfo

        public string EntityName { get { throw new NotImplementedException(); } }
        public Type EntityType { get; }
        public Dictionary<string, ForeignKeyAttribute> ForeignKeys { get { throw new NotImplementedException(); } }
        public Dictionary<string, SecondaryKeyAttribute> SecondaryKeys { get { throw new NotImplementedException(); } }
        public Dictionary<string, Type> PersistedProperties { get { throw new NotImplementedException(); } }

        #endregion


        #region Implementation of IAdoEntityInfo

        public IAdoNetRepository Repository { get; internal set; }
        public TimeSpan CacheTimeout { get { throw new NotImplementedException(); } }
        public string[] ReaderColumns { get; private set; }


        public int IndexOf(string columnName) {
            throw new NotImplementedException();
        }


        public void InitReaderColumns(DbDataReader reader) {
            if (ReaderColumns != null)
                return;

#if NET40
            var columns = reader.GetSchemaTable()?.Rows.Cast<DataRow>().Select(x => (string) x[0]).ToArray();
#else
            throw new NotImplementedException();
#endif
            lock (_lock) {
                if (ReaderColumns == null)
                    ReaderColumns = columns;
            }
        }

        #endregion
    }
}