using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Ado {
    

    static class DataReaderExtensions {
        
        public static IEnumerable<object> SelectColumn(this DbDataReader reader, int columnIndex) {
            while (reader.Read()) {
                yield return reader.GetValue(columnIndex);
            }
        }


        public static int RowsCount(this DbDataReader reader) {
            var count = 0;
            while (reader.Read()) {
                count++;
            }
            return count;
        }


        static IEnumerable<object> ToOneObject(this DbDataReader reader, IAdoEntityInfo info) {
            info.InitReaderColumns(reader);

            while (reader.Read()) {
                object keyId;
                yield return reader.ReadItem(info, out keyId);
            }
        }



        public static IEnumerable<object> To(this DbDataReader reader, IAdoEntityInfo mainObjectInfo) {

            if (mainObjectInfo.Children == null)
                return reader.ToOneObject(mainObjectInfo);


            var mainObjects = new Dictionary<object, object>();

            mainObjectInfo.InitReaderColumns(reader);

            while (reader.Read()) {
                object keyId;
                var item = reader.ReadItem(mainObjectInfo, out keyId);
                try {
                    mainObjects.Add(keyId, item);
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Critical,
                        $"On reading '{mainObjectInfo.EntityType}' using special name {mainObjectInfo.EntityName} getted exception, possibly because reader contains more then one object with same idenifier.\n" +
                        $"Identifier: {keyId}\n" +
                        $"Exception: {e}");

                    throw;
                }
            }

            var includeInformation = mainObjectInfo.Children;

            for (var resultIdx = 0; resultIdx < includeInformation.Length; resultIdx++) {
                if (!reader.NextResult()) {
                    Internal.TraceEvent(TraceEventType.Warning, $"Object {mainObjectInfo.EntityType.FullName} has includedInformation for #{includeInformation.Length} items, but reader returned only {reader.Depth} tables (including main objects)");
                    return mainObjects.Values;
                }

                var info = includeInformation[resultIdx];

                InitColumnNames(info, reader);

                Dictionary<object, IList> children = null;
                if (info.IsCollection)
                    children = new Dictionary<object, IList>();

                while (reader.Read()) {
                    object keyId;
                    var item = ReadItem(reader, info, out keyId);

                    if (keyId == null) {
                        Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.Type}, {info.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                        break;
                    }

                    if (!info.IsCollection) {
                        Parallel.ForEach(mainObjects.Values,
                            mainObject => {
                                object value;
                                if (!mainObject.TryGetValueOnPath(info.ChildIdKey, out value) || !keyId.Equals(value))
                                    return;

                                if (!mainObject.TrySetValueOnPath(info.TargetPath, item))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {info.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.Type} in result set #{resultIdx}.");
                            });

/*
                                mainObjects.Values.Where(x => {
                                    object value;
                                    return RUtils.TryGetValueOnPath(x, info.ChildIdKey, out value) && value == keyId;
                                }).Perform(x => {
                                    if (!RUtils.TrySetValueOnPath(x, info.TargetPath, item))
                                        Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {info.TargetPath} from '{mainObjectInfo.Type.FullName}' context.\nTarget path specified for child item {info.Type} in result set #{resultIdx}.");
                                }).ToEnd();
*/


/*
                                object mainObject;
                                if (!mainObjects.TryGetValue(keyId, out mainObject)) {
                                    Internal.TraceEvent(TraceEventType.Warning, $"In result set #{resultIdx} finded data row with item {item}, that doesn't has owner object in result set #1.\nOwner Id is {keyId} ({info.KeyIdName}).\nTarget path is '{info.TargetPath}'.");
                                    continue;
                                }
                                if (!RUtils.TrySetValueOnPath(mainObject, info.TargetPath, item))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {info.TargetPath} from '{mainObjectInfo.Type.FullName}' context.\nTarget path specified for child item {info.Type} in result set #{resultIdx}.");
                                
                                if (!string.IsNullOrWhiteSpace(info.OwnerReferenceName))
                                    if (!RUtils.TrySetValueOnPath(item, info.OwnerReferenceName, mainObject))
                                        Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {info.OwnerReferenceName} from '{info.Type}' context. This should be reference to owner object ({mainObject})");
*/

                        continue;
                    }

                    IList collection;
                    if (!children.TryGetValue(keyId, out collection)) {
                        collection = (IList) Activator.CreateInstance(info.TargetCollectionType);
                        children.Add(keyId, collection);
                    }

                    collection.Add(item);
                }

                if (info.IsCollection) {
                    Parallel.ForEach(children,
                        child => {
                            object mainObject;
                            if (!mainObjects.TryGetValue(child.Key, out mainObject)) {
                                Internal.TraceEvent(TraceEventType.Warning, $"In result set #{resultIdx} finded data row of type {info.Type}, that doesn't has owner object in result set #1.\nOwner Id is {child.Key} ({info.KeyId.Name}).\nTarget path is '{info.TargetPath}'.");
                                return;
                            }

                            if (!mainObject.TrySetValueOnPath(info.TargetPath, child.Value))
                                Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {info.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.Type} in result set #{resultIdx}.");

                            if (string.IsNullOrWhiteSpace(info.OwnerReferenceName))
                                return;

                            foreach (var item in child.Value) {
                                if (!item.TrySetValueOnPath(info.OwnerReferenceName, mainObject))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {info.OwnerReferenceName} from '{info.Type}' context. This should be reference to owner object ({mainObject})");
                            }
                        });
                }
            }


            return mainObjects.Values;
        }


        static object ReadItem(this DbDataReader reader, IAdoEntityInfo info, out object keyId) {
            var item = Activator.CreateInstance(info.EntityType);

            keyId = null;

            for (var i = 0; i < info.ReaderColumns.Length; i++) {
                var name = info.ReaderColumns[i];
                var value = reader.GetValue(i);
                if (value == null || value.GetType().FullName == "System.DBNull")
                    continue;

                if (name == info.KeyId.Name) //todo: move all of these methods to base AdoNetRepository
                    keyId = value;

                if (!item.TrySetValueOnPath(name, value))
                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {name} from '{info.EntityType.FullName}' context.");
            }
            return item;
        }


/*
        static void InitColumnNames(IAdoEntityInfo info, DbDataReader reader) {
            if (info.ReaderColumns != null)
                return;

            lock (info) {
                if (info.ReaderColumns != null)
                    return;

#if NET40
                info.ReaderColumns = reader.GetSchemaTable()?.Rows.Cast<DataRow>().Select(x => (string)x[0]).ToArray();
#else
                throw new NotImplementedException();
#endif
            }
        }
*/

    }






}