using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Ado.Implementation;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado {
    

    /// <summary>
    /// Am <see cref="IAdoNetRepository"/> what defines special names of CRUD-operation stored procedures.
    /// </summary>
    /// <seealso cref="NameOfReadProcedureFor"/>
    /// <seealso cref="NameOfInsertProcedureFor"/>
    /// <seealso cref="NameOfUpdateProcedureFor"/>
    /// <seealso cref="NameOfDeleteProcedureFor"/>
    public abstract class AdoNetRepository<TCommand, TParameter, TConnection> : IAdoNetRepository
        where TCommand : DbCommand
        where TParameter: DbParameter
        where TConnection : DbConnection {


        protected abstract TParameter PrimaryKeyParameterOf<TEntity>(IEntityInfo info, TEntity entity);

        protected abstract TConnection GetConnection();

        protected abstract TParameter ParameterWith(string name, Type type, object value);

        protected abstract TCommand CommandFor(string commandText, TConnection connection, IEnumerable<TParameter> parameters, CommandType type = CommandType.StoredProcedure);

        
        protected virtual CommandBehavior GetBehaviorFor(IEntityInfo info) {
            if (info.Inclusions?.Any() != true)
                return CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;

            return CommandBehavior.SequentialAccess;
        }

        
        protected virtual string NameOfReadProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_get_by_criteria");
        }


        
        protected virtual string NameOfInsertProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_insert");
        }


        
        protected virtual string NameOfUpdateProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_update");
        }


        
        protected virtual string NameOfDeleteProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_delete");
        }


        protected virtual IEnumerable<TParameter> ParametersFrom<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            return info.PersistedProperties.Select(x => {
                object value;
                if (!RUtils<TEntity>.TryGetValueOnPath(entity, x.Key, out value))
                    throw new InvalidOperationException($"Entity of type {entity.GetType()} should contain property '{x.Key}', " +
                                                        $"but in some reason value or that property could not be getted.");

                return ParameterWith(x.Key, x.Value.PropertyType, value);
            });
        }
        


        /// <summary>
        /// Returns reader that can be used to get some data by <paramref name="entityName"/>, filtering it by <paramref name="criteria"/>.
        /// </summary>
        /// <param name="entityName">Database-specific name of some entity</param>
        /// <param name="criteria">Filtering criteria</param>
        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read((IAdoEntityInfo) info, criteria);
        }

        protected virtual IEnumerable<object> Read(IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            if (info.Inclusions?.Any() != true)
                return GetMainEntities(info, parameters);;

            return ReadToEnd(info, parameters);
/*

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return ReadToEnd(reader, info);
            }
*/
        }


        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        /// <exception cref="Exception">.</exception>
        public bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            var behavior = GetBehaviorFor(info) | CommandBehavior.SingleRow;

            var connection = GetConnection();
            try {
                using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader(behavior)) {
                        return reader.Read();
                    }
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }
        


        public IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return ReadColumn(columnName, (IAdoEntityInfo) info, criteria);
        }


        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        protected virtual IEnumerable<object> ReadColumn(string columnName, IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            var idx = info.ReaderColumns != null ? info.IndexOf(columnName) : -1;

            var behavior = GetBehaviorFor(info);
            var connection = GetConnection();
            try {
                using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader(behavior)) {
                        if (idx < 0) {
                            info.InitReaderColumns(reader);
                            idx = info.IndexOf(columnName);
                        }

                        while (reader.Read()) {
                            yield return reader.GetValue(idx);
                        }
                    }
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            var behavior = GetBehaviorFor(info);
            var connection = GetConnection();
            try {
                using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader(behavior)) {
                        return RowsCount(reader);
                    }
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// Inserts an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool Insert<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            var parameters = ParametersFrom(info, entity);
            var connection = GetConnection();
            try {
                using (var command = CommandFor(NameOfInsertProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfInsertProcedureFor(info.EntityName);
            var connection = GetConnection();

            try {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(info, entity);

                    using (var command = CommandFor(name, connection, parameters)) {
                        if (command.ExecuteNonQuery() <= 0) {
                            return false;
                        }
                    }
                }

                return true;
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// Updates an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool Update<T>(IEntityInfo info, T entity) where T : class {
            var parameters = ParametersFrom(info, entity);
            var connection = GetConnection();
            try {
                using (var command = CommandFor(NameOfUpdateProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfUpdateProcedureFor(info.EntityName);
            var connection = GetConnection();
            try {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(info, entity);

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0) {
                            return false;
                        }
                    }
                }

                return true;
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// Removes an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool Delete<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            var parameters = new [] { PrimaryKeyParameterOf(info, entity) };
            var connection = GetConnection();
            try {
                using (var command = CommandFor(NameOfDeleteProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfDeleteProcedureFor(info.EntityName);
            var connection = GetConnection();
            try {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = new[] {PrimaryKeyParameterOf(info, entity)};

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
                            return false;
                    }
                }

                return true;
            }
            finally {
                connection.Close();
                connection.Dispose();
            }

        }





        /*
        protected virtual IEnumerable<object> SelectColumn(DbDataReader reader, int columnIndex) {
            while (reader.Read()) {
                yield return reader.GetValue(columnIndex);
            }
        }
*/

        protected virtual int RowsCount(DbDataReader reader) {
            var count = 0;
            while (reader.Read()) {
                count++;
            }
            return count;
        }


        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        protected virtual IEnumerable<object> GetMainEntities(IAdoEntityInfo info, IEnumerable<TParameter> parameters) {
            var connection = GetConnection();
            var behavior = GetBehaviorFor(info);
            try {
                using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader(behavior)) {
                        info.InitReaderColumns(reader);

                        while (reader.Read()) {
                            object keyId;
                            yield return ReadItem(reader, info, null, out keyId);
                        }
                    }
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }


        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        protected virtual IEnumerable<object> ReadToEnd(IAdoEntityInfo mainObjectInfo, IEnumerable<TParameter> parameters) {
            var connection = GetConnection();

            var behavior = GetBehaviorFor(mainObjectInfo);
            try {
                using (var command = CommandFor(NameOfReadProcedureFor(mainObjectInfo.EntityName), connection, parameters)) {
                    connection.Open();
                    using (var reader = command.ExecuteReader(behavior)) {

                        mainObjectInfo.InitReaderColumns(reader);

                        var mainObjects = new Dictionary<object, object>();

                        while (reader.Read()) {
                            object keyId;
                            var item = ReadItem(reader, mainObjectInfo, null, out keyId);
                            try {
                                mainObjects.Add(keyId, item);
                            }
                            catch (Exception e) {
                                Internal.TraceEvent(TraceEventType.Critical,
                                    $"On reading '{mainObjectInfo.EntityType}' using special name {mainObjectInfo.EntityName} getted exception, " +
                                    "possibly because reader contains more then one object with same identifier.\n" +
                                    $"Identifier: {keyId}\n" +
                                    $"Exception: {e}");

                                throw;
                            }
                        }


                        var tableIndex = 0;

                        while (reader.NextResult()) {
                            tableIndex++;

                            Includeon includeon;
                            if (!mainObjectInfo.Inclusions.TryGetValue(tableIndex, out includeon)) {
                                Internal.TraceEvent(TraceEventType.Warning,
                                    $"Reader for object {mainObjectInfo.EntityType.FullName} returned more than one table, " +
                                    $"but it has not includeon information for table#{tableIndex}.");
                                continue;
                            }

                            var info = includeon.Info;

                            info.InitReaderColumns(reader);


                            if (!includeon.IsCollection) {
                                while (reader.Read()) {
                                    object keyId;
                                    var item = ReadItem(reader, mainObjectInfo, includeon, out keyId);

                                    if (keyId == null) {
                                        Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.EntityType}, {includeon.Attribute.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                                        break;
                                    }

                                    var index = tableIndex;
                                    Parallel.ForEach(mainObjects.Values,
                                        mainObject => {
                                            object value;
                                            if (!mainObject.TryGetValueOnPath(includeon.ForeignKeyFromMainEntityToCurrent, out value) || !keyId.Equals(value)) // target path should be ServiceWorkplaceId, but it ServiceWorkPlace
                                                return; //keyId should be just primary key of ServiceWorkPlace

                                            if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, item))
                                                Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");
                                        });
                                }

                            }
                            else {
                                var children = new Dictionary<object, IList>(); //? Key is Main Object Primary Key, Value is children collection of main object's navigation property

                                while (reader.Read()) {
                                    object keyId;
                                    var item = ReadItem(reader, mainObjectInfo, includeon, out keyId);

                                    if (keyId == null) {
                                        Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.EntityType}, {includeon.Attribute.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                                        break;
                                    }


                                    IList collection;
                                    if (!children.TryGetValue(keyId, out collection)) {
                                        collection = (IList) Activator.CreateInstance(includeon.TargetCollectionType);
                                        children.Add(keyId, collection);
                                    }

                                    collection.Add(item);
                                }


                                var index = tableIndex;
                                Parallel.ForEach(children,
                                    child => {
                                        object mainObject;
                                        if (!mainObjects.TryGetValue(child.Key, out mainObject)) {
                                            Internal.TraceEvent(TraceEventType.Warning, $"In result set #{index} finded data row of type {info.EntityType}, that doesn't has owner object in result set #1.\nOwner Id is {child.Key}.\nTarget path is '{includeon.Attribute.TargetPath}'.");
                                            return;
                                        }

                                        if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, child.Value))
                                            Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");

                                        if (string.IsNullOrWhiteSpace(includeon.OwnerNavigationReferenceName))
                                            return;

                                        foreach (var item in child.Value) {
                                            if (!item.TrySetValueOnPath(includeon.OwnerNavigationReferenceName, mainObject))
                                                Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.OwnerNavigationReferenceName} from '{info.EntityType}' context. This should be reference to owner object ({mainObject})");
                                        }
                                    });
                            }
                        }



                        return mainObjects.Values;


                    }
                }
            }
            finally {
                connection.Close();
                connection.Dispose();
            }
        }

/*

        protected virtual IEnumerable<object> ReadToEnd(DbDataReader reader, IAdoEntityInfo mainObjectInfo) {

            if (mainObjectInfo?.Inclusions.Any() != true)
                return GetMainEntities(reader, mainObjectInfo);


            var mainObjects = new Dictionary<object, object>();

            mainObjectInfo.InitReaderColumns(reader);

            while (reader.Read()) {
                object keyId;
                var item = ReadItem(reader, mainObjectInfo, null, out keyId);
                try {
                    mainObjects.Add(keyId, item);
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Critical,
                        $"On reading '{mainObjectInfo.EntityType}' using special name {mainObjectInfo.EntityName} getted exception, " +
                        "possibly because reader contains more then one object with same identifier.\n" +
                        $"Identifier: {keyId}\n" +
                        $"Exception: {e}");

                    throw;
                }
            }


            var tableIndex = 0;

            while (reader.NextResult()) {
                tableIndex++;

                Includeon includeon;
                if (!mainObjectInfo.Inclusions.TryGetValue(tableIndex, out includeon)) {
                    Internal.TraceEvent(TraceEventType.Warning, $"Reader for object {mainObjectInfo.EntityType.FullName} returned more than one table, " +
                                                                $"but it has not includeon information for table#{tableIndex}.");
                    continue;
                }

                var info = includeon.Info;

                info.InitReaderColumns(reader);


                if (!includeon.IsCollection) {
                    while (reader.Read()) {
                        object keyId;
                        var item = ReadItem(reader, mainObjectInfo, includeon, out keyId);

                        if (keyId == null) {
                            Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.EntityType}, {includeon.Attribute.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                            break;
                        }

                        var index = tableIndex;
                        Parallel.ForEach(mainObjects.Values,
                            mainObject => {
                                object value;
                                if (!mainObject.TryGetValueOnPath(includeon.ForeignKeyFromMainEntityToCurrent, out value) || !keyId.Equals(value)) // target path should be ServiceWorkplaceId, but it ServiceWorkPlace
                                    return; //keyId should be just primary key of ServiceWorkPlace

                                if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, item))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");
                            });
                    }

                }
                else {
                    var children = new Dictionary<object, IList>(); //? Key is Main Object Primary Key, Value is children collection of main object's navigation property

                    while (reader.Read()) {
                        object keyId;
                        var item = ReadItem(reader, mainObjectInfo, includeon, out keyId);

                        if (keyId == null) {
                            Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.EntityType}, {includeon.Attribute.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                            break;
                        }


                        IList collection;
                        if (!children.TryGetValue(keyId, out collection)) {
                            collection = (IList) Activator.CreateInstance(includeon.TargetCollectionType);
                            children.Add(keyId, collection);
                        }

                        collection.Add(item);
                    }


                    var index = tableIndex;
                    Parallel.ForEach(children,
                        child => {
                            object mainObject;
                            if (!mainObjects.TryGetValue(child.Key, out mainObject)) {
                                Internal.TraceEvent(TraceEventType.Warning, $"In result set #{index} finded data row of type {info.EntityType}, that doesn't has owner object in result set #1.\nOwner Id is {child.Key}.\nTarget path is '{includeon.Attribute.TargetPath}'.");
                                return;
                            }

                            if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, child.Value))
                                Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");

                            if (string.IsNullOrWhiteSpace(includeon.OwnerNavigationReferenceName))
                                return;

                            foreach (var item in child.Value) {
                                if (!item.TrySetValueOnPath(includeon.OwnerNavigationReferenceName, mainObject))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.OwnerNavigationReferenceName} from '{info.EntityType}' context. This should be reference to owner object ({mainObject})");
                            }
                        });
                }
            }

            

            return mainObjects.Values;
        }
*/


        protected virtual object ReadItem(DbDataReader reader, IAdoEntityInfo mainEntityInfo, [CanBeNull] Includeon includeon, out object keyId) {
            var entityType = includeon?.Info.EntityType ?? mainEntityInfo.EntityType;

            var item = Activator.CreateInstance(entityType);

            keyId = null;

            var isKey = includeon == null
                ? IsPrimaryKey
                : includeon.IsCollection
                    ? (Func<string, IAdoEntityInfo, Includeon, bool>) IsKeyOfMainEntityInForeign
                    : IsPrimaryKey;


            var columns = includeon?.Info.ReaderColumns ?? mainEntityInfo.ReaderColumns;

            for (var i = 0; i < columns.Length; i++) {
                var name = columns[i];
                var value = reader.GetValue(i);
                if (value == null || value.GetType().FullName == "System.DBNull")
                    continue;

                if (isKey(name, mainEntityInfo, includeon))
                    keyId = value;

                if (!item.TrySetValueOnPath(name, value))
                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {name} from '{entityType.FullName}' context.");
            }
            return item;
        }

/*

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="mainEntityInfo">
        ///     If <see langword="null"/> - <paramref name="currentEntityInfo"/> is information about main object 
        ///     and <paramref name="propertyName"/> is main object's property, 
        ///     if not - <paramref name="currentEntityInfo"/> is information about child of main object
        /// </param>
        /// <param name="currentEntityInfo"></param>
        /// <returns>
        ///     Is property with givent <paramref name="propertyName"/> are primary key of main object (then it's property of main object),
        ///     or is that property are foreign key from child object to main (if includeon is <paramref name="currentEntityInfo.IsCollection"/>),
        ///     or is that property are foreign key from main object to other (if current includeon is not collection).
        /// </returns>
        protected abstract bool IsMainObjectKey(string propertyName, [CanBeNull] IAdoEntityInfo mainEntityInfo, IAdoEntityInfo currentEntityInfo); //todo: rename, because it's not always PrimaryKey

*/

        protected abstract bool IsPrimaryKey([NotNull] string propertyName, [NotNull]  IAdoEntityInfo mainEntityInfo,[CanBeNull] Includeon includeon);


        protected virtual bool IsForeignKeyToOtherEntityFromMain([NotNull] string propertyName, [NotNull] IAdoEntityInfo mainEntityInfo, [NotNull] Includeon includeon) {
            return propertyName == includeon.ForeignKeyFromMainEntityToCurrent;
        }


        protected virtual bool IsKeyOfMainEntityInForeign([NotNull] string propertyName, [NotNull] IAdoEntityInfo mainEntityInfo, [NotNull] Includeon includeon) {
            return propertyName == includeon.ForeignKeyFromCurrentEntityToMain;
        }

    }
}