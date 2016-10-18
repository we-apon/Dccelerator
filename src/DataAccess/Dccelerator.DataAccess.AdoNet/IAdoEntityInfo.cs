using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Ado {

    public interface IAdoEntityInfo<out TDbTypeEnum> : IAdoEntityInfo
        where TDbTypeEnum : struct {

        TDbTypeEnum GetDefaultDbType(Type type);

        TDbTypeEnum GetParameterDbType(string parameterName);
    }

    public interface IAdoEntityInfo : IEntityInfo {
        
        IAdoNetRepository Repository { get; }

        TimeSpan CacheTimeout { get; }
        string[] ReaderColumns { get; }

        int IndexOf(string columnName);

        void InitReaderColumns(DbDataReader reader);

        new Dictionary<int, Includeon> Inclusions { get; }

        void SetupRepository(IAdoNetRepository repository);

        /// <summary>
        /// Cached insert sql queries. Key is FullName of repository type.
        /// </summary>
        ConcurrentDictionary<string, string> CachedInsertQueries { get; }

        /// <summary>
        /// Cached update sql queries. Key is FullName of repository type.
        /// </summary>
        ConcurrentDictionary<string, string> CachedUpdateQueries { get; }

        /// <summary>
        /// Cached delete sql queries. Key is FullName of repository type.
        /// </summary>
        ConcurrentDictionary<string, string> CachedDeleteQueries { get; }

    }

}