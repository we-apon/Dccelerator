using System;
using System.Collections.Generic;
using System.Data.Common;


namespace Dccelerator.DataAccess.Ado.Infrastructure {
    public interface IAdoEntityInfo : IEntityInfo {
        
        IAdoNetRepository Repository { get; }

        TimeSpan CacheTimeout { get; }
        string[] ReaderColumns { get; }

        int IndexOf(string columnName);

        void InitReaderColumns(DbDataReader reader);


        Dictionary<int, Includeon> Inclusions { get; }
    }

/*

    public interface IEntityInfo {


        Type Type { get; }


#if NET40
        Type TypeInfo { get; }
#else
        TypeInfo TypeInfo { get; }
#endif


        IEntityInfo[] Children { get; }

        string EntityName { get; }


        /// <summary>
        /// Contains name and type of key id field of entity.
        /// Can be null, because syntetic entities may hasn't unique identifier.
        /// </summary>
        PropertyInfo KeyId { get; }


        /// <summary>
        /// Contains pairs of names and types of all persisted fields
        /// </summary>
        Tuple<string, Type>[] PersistedFields { get; }


        bool AllowCache { get; }
        TimeSpan CacheTimeout { get; }
        Type RealRepositoryType { get; }

        IInternalReadingRepository GlobalReadingRepository { get; }

        IAdoNetRepository RealRepository { get; }
        EntityAttribute EntityAttribute { get; }






         bool IsCollection { get; }
         string[] ColumnNames { get; set; }
         // string KeyIdName { get; }
         string ChildIdKey { get; }
         string TargetPath { get; }
         string OwnerReferenceName { get; }
         Type TargetCollectionType { get; }
    }

*/

}