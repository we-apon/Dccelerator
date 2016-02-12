using System;
using System.Collections.Generic;
using System.Data.Common;

/*
    private void SetupLazyContext(object obj)
    {
        SetupLazyContext(obj, Info);
    }


    private void SetupLazyContext(object obj, EntityInfo info, Entity ownerObject = null)
    {
        var entity = obj as Entity;
        if (entity != null)
        {
            if (ownerObject == null)
                entity.Internal_AllowLazyLoading(Repository);
            else {
                entity.CopyLoadingContextFrom(ownerObject);
            }
        }
        else {
            var lazyEntity = obj as ILazyEntity;
            lazyEntity?.AllowLazyLoading();
        }

        if (info.Children == null)
            return;

        foreach (var childInfo in info.Children)
        { // try to use parallels
            object child;
            if (!TypeInfo.TryGetNestedProperty(obj, childInfo.TargetPath, out child))
            {
                Internal.TraceEvent(TraceEventType.Warning, $"Can't get property {childInfo.TargetPath} on {entity.GetType()} to setup it's loading context.");
                continue;
            }

            if (childInfo.IsCollection)
            {
                foreach (var item in (IEnumerable)child)
                {
                    SetupLazyContext(item, childInfo, entity);
                }
            }
            else {
                SetupLazyContext(child, childInfo, entity);
            }
        }
    }


*/

namespace Dccelerator.DataAccess.Ado.Infrastructure {
    public interface IAdoEntityInfo : IEntityInfo {
        
        IAdoNetRepository Repository { get; }

        TimeSpan CacheTimeout { get; }
        string[] ReaderColumns { get; }

        int IndexOf(string columnName);

        void InitReaderColumns(DbDataReader reader);


        new Dictionary<int, Includeon> Inclusions { get; }
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