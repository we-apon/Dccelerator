using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Ado.Implementation;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado {
    /// <summary>
    /// Am <see cref="IAdoNetRepository"/> what defines special names of CRUD-operation stored procedures.
    /// </summary>
    /// <seealso cref="ReadCommandText"/>
    /// <seealso cref="InsertCommandText{TEntity}"/>
    /// <seealso cref="UpdateCommandText{TEntity}"/>
    /// <seealso cref="DeleteCommandText{TEntity}"/>
    public abstract class AdoNetRepository<TCommand, TParameter, TConnection> : IAdoNetRepository
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TConnection : DbConnection {

        DbConnection IAdoNetRepository.GetConnection() => GetConnection();

        protected internal abstract string DatabaseSpecificNameOf(string parameter);

        Type _repositoryType;
        protected Type RepositoryType => _repositoryType ?? (_repositoryType = GetType());

        /// <summary>
        /// Get instance of <see cref="TParameter" /> that represents primary key.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="info">Information about entity</param>
        /// <param name="entity">Instance of entity</param>
        /// <returns>Return instance of parameter</returns>
        protected internal abstract TParameter PrimaryKeyParameterOf<TEntity>(IEntityInfo info, TEntity entity);

        /// <summary> 
        /// Get instance <see cref="TConnection" /> for database.
        /// </summary>
        /// <remarks>Connection should be instantiated, but unopened.</remarks>
        /// <returns>Return instance of connection</returns>
        protected internal abstract TConnection GetConnection();

        /// <summary>
        /// Get instance of <see cref="TParameter" /> for queries based on input info and entity.
        /// </summary>
        /// <param name="info">Information about entity</param>
        /// <param name="criterion">Criterion that should be represented by returned <typeparamref name="TParameter"/></param>
        /// <returns>Return instance of <typeparamref name="TParameter"/> </returns>
        protected internal abstract TParameter ParameterWith(IEntityInfo info, IDataCriterion criterion);

        /// <summary>
        /// Represents a command that will be executed against a database.
        /// </summary>
        /// <param name="commandText">String that represent query for command</param>
        /// <param name="connection">Instance of connection</param>
        /// <param name="parameters">List of parameters</param>
        /// <param name="type">Type of command</param>
        /// <returns>Return instance of command</returns>
        protected internal abstract TCommand CommandFor(string commandText, TConnection connection, IEnumerable<TParameter> parameters, CommandType type = CommandType.StoredProcedure);


        /// <summary>
        /// Get text for <typeparamref name="TCommand"/> used for reading data.
        /// </summary>
        /// <param name="info">Information about entity</param>
        /// <param name="criteria">Query criteria</param>
        /// <returns>Return string with query</returns>
        protected internal virtual string ReadCommandText(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            return info.UsingQueries
                ? SelectQuery(info, criteria)
                : SelectProcedure(info, criteria);
        }


        /// <summary>
        /// Get text for <typeparamref name="TCommand"/> used for inserting <paramref name="entity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="info">Information about entity</param>
        /// <param name="entity">Instance of entity</param>
        /// <returns>Return string with query</returns>
        protected internal virtual string InsertCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return info.UsingQueries
                ? InsertQuery(info, entity)
                : InsertProcedure(info, entity);
        }

        /// <summary>
        /// Get text for <typeparamref name="TCommand"/> used for updating <paramref name="entity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="info">Information about entity</param>
        /// <param name="entity">Instance of entity</param>
        /// <returns>Return string with query</returns>
        protected internal virtual string UpdateCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return info.UsingQueries
                ? UpdateQuery(info, entity)
                : UpdateProcedure(info, entity);
        }


        /// <summary>
        /// Get text for <typeparamref name="TCommand"/> used for deleting <paramref name="entity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="info">Information about entity</param>
        /// <param name="entity">Instance of entity</param>
        /// <returns>Return string with query</returns>
        protected internal virtual string DeleteCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return info.UsingQueries
                ? DeleteQuery(info, entity)
                : DeleteProcedure(info, entity);
        }


        protected internal virtual string SelectQuery(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            var builder = new StringBuilder("select * from ").Append(info.EntityName).Append(" where ");

            foreach (var criterion in criteria)
                builder.Append(criterion.Name).Append(" = ").Append(DatabaseSpecificNameOf(criterion.Name)).Append(" and ");

            return builder.Remove(builder.Length - 5, 5).ToString();
        }

        protected internal virtual string InsertQuery<TEntity>(IEntityInfo info, TEntity entity) {
            var adoInfo = (IAdoEntityInfo) info;

            string query;
            if (adoInfo.CachedInsertQueries.TryGetValue(RepositoryType.FullName, out query))
                return query;

            var builder = new StringBuilder("insert into ").Append(info.EntityName).Append(" ( ");

            foreach (var property in info.PersistedProperties.Keys)
                builder.Append(property).Append(", ");

            builder.Remove(builder.Length - 2, 2).Append(" )\n values ( ");

            foreach (var property in info.PersistedProperties.Keys)
                builder.Append(DatabaseSpecificNameOf(property)).Append(", ");

            query = builder.Remove(builder.Length - 2, 1).Append(")").ToString();

            adoInfo.CachedInsertQueries.TryAdd(RepositoryType.FullName, query);
            return query;
        }

        protected internal virtual string UpdateQuery<TEntity>(IEntityInfo info, TEntity entity) {
            var adoInfo = (IAdoEntityInfo)info;

            string query;
            if (adoInfo.CachedUpdateQueries.TryGetValue(RepositoryType.FullName, out query))
                return query;

            var builder = new StringBuilder("update ").Append(info.EntityName).Append(" set ");
            var key = PrimaryKeyParameterOf(info, entity);

            foreach (var property in info.PersistedProperties.Keys) {
                if (property == key.ParameterName) continue;
                builder.Append(property).Append(" = ").Append(DatabaseSpecificNameOf(property)).Append(", ");
            }

            builder.Remove(builder.Length - 2, 2);
            builder.Append(" where ").Append(key.ParameterName).Append(" = ").Append(DatabaseSpecificNameOf(key.ParameterName));
            query = builder.ToString();

            adoInfo.CachedUpdateQueries.TryAdd(RepositoryType.FullName, query);
            return query;
        }

        protected internal virtual string DeleteQuery<TEntity>(IEntityInfo info, TEntity entity) {
            var adoInfo = (IAdoEntityInfo)info;

            string query;
            if (adoInfo.CachedDeleteQueries.TryGetValue(RepositoryType.FullName, out query))
                return query;

            var builder = new StringBuilder("delete from ").Append(info.EntityName);

            var key = PrimaryKeyParameterOf(info, entity);

            builder.Append(" where ").Append(key.ParameterName).Append(" = ").Append(DatabaseSpecificNameOf(key.ParameterName));
            query = builder.ToString();

            adoInfo.CachedDeleteQueries.TryAdd(RepositoryType.FullName, query);
            return query;
        }

        protected internal virtual string SelectProcedure(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            return string.Concat("obj_", info.EntityName, "_get_by_criteria");
        }

        protected internal virtual string InsertProcedure<TEntity>(IEntityInfo info, TEntity entity) {
            return string.Concat("obj_", info.EntityName, "_insert");
        }

        protected internal virtual string UpdateProcedure<TEntity>(IEntityInfo info, TEntity entity) {
            return string.Concat("obj_", info.EntityName, "_update");
        }

        protected internal virtual string DeleteProcedure<TEntity>(IEntityInfo info, TEntity entity) {
            return string.Concat("obj_", info.EntityName, "_delete");
        }

        /// <summary>
        /// Get <see cref="CommandBehavior"/> for command.
        /// </summary>
        /// <returns>Instance of CommandBehavior</returns>
        protected virtual CommandBehavior GetBehaviorFor(IEntityInfo info) {
            if (info.Inclusions?.Any() != true)
                return CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;

            return CommandBehavior.SequentialAccess;
        }

        /// <summary>
        /// Returns whole <paramref name="entity"/>, represented as sequence of <typeparamref name="TParameter"/>.
        /// </summary>
        /// <returns>IEnumerable (sequence) of TParameter</returns>
        protected virtual IEnumerable<TParameter> ParametersFrom<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            return info.PersistedProperties.Select(x => {
                object value;
                if (!RUtils<TEntity>.TryGetValueOnPath(entity, x.Key, out value))
                    throw new InvalidOperationException($"Entity of type {entity.GetType()} should contain property '{x.Key}', " +
                                                        "but in some reason value or that property could not be getted.");

                var criterion = new DataCriterion {Name = x.Key, Type = x.Value.PropertyType, Value = x.Value};
                return ParameterWith(info, criterion);
            });
        }


        /// <summary>
        /// Returns entities from database, filtered with <paramref name="criteria"/>.
        /// </summary>
        /// <param name="info">Information about entity</param>
        /// <param name="criteria">Filtering criteria</param>
        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read((IAdoEntityInfo) info, criteria);
        }

        /// <summary>
        /// Returns entities from database, filtered with <paramref name="criteria"/>.
        /// </summary>
        /// <param name="info">Information about entity</param>
        /// <param name="criteria">Filtering criteria</param>
        protected virtual IEnumerable<object> Read(IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            return info.Inclusions?.Any() == true
                ? ReadToEnd(info, criteria)
                : GetMainEntities(info, criteria);
        }

        /// <summary>
        /// Checks, is database contains any entity (specified by it's <paramref name="info"/>), that satisfying passed filtering <paramref name="criteria"/>.
        /// </summary>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(info, x));

            var behavior = GetBehaviorFor(info) | CommandBehavior.SingleRow;

            var connection = GetConnection();
            try {
                using (var command = CommandFor(ReadCommandText(info, criteria), connection, parameters)) {
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


        /// <summary>
        /// Returns values of one property/column of entities, that satisfying passed filtering <paramref name="criteria"/>.
        /// </summary>
        public IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return ReadColumn(columnName, (IAdoEntityInfo) info, criteria);
        }


        /// <summary>
        /// Returns values of one property/column of entities, that satisfying passed filtering <paramref name="criteria"/>.
        /// </summary>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        protected virtual IEnumerable<object> ReadColumn(string columnName, IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(info, x));

            var idx = info.ReaderColumns != null ? info.IndexOf(columnName) : -1;

            var behavior = GetBehaviorFor(info);
            var connection = GetConnection();
            try {
                using (var command = CommandFor(ReadCommandText(info, criteria), connection, parameters)) {
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


        /// <summary>
        /// Get number of entities, that satisfying passed filtering <paramref name="criteria"/>.
        /// </summary>
        /// <returns>Number of entities</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(info, x));

            var behavior = GetBehaviorFor(info);
            var connection = GetConnection();
            try {
                using (var command = CommandFor(ReadCommandText(info, criteria), connection, parameters)) {
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
        public virtual bool Insert<TEntity>(IEntityInfo info, TEntity entity, DbConnection connection) where TEntity : class {
            var parameters = ParametersFrom(info, entity);

            using (var command = CommandFor(InsertCommandText(info, entity), (TConnection) connection, parameters)) {
                return command.ExecuteNonQuery() > 0;
            }
        }


        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities, DbConnection connection) where TEntity : class {
            var name = InsertCommandText(info, entities);


            foreach (var entity in entities) {
                var parameters = ParametersFrom(info, entity);

                using (var command = CommandFor(name, (TConnection) connection, parameters)) {
                    if (command.ExecuteNonQuery() <= 0) {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Updates an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool Update<T>(IEntityInfo info, T entity, DbConnection connection) where T : class {
            var parameters = ParametersFrom(info, entity);
            using (var command = CommandFor(UpdateCommandText(info, entity), (TConnection) connection, parameters)) {
                return command.ExecuteNonQuery() > 0;
            }
        }


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities, DbConnection connection) where TEntity : class {
            var name = UpdateCommandText(info, entities);

            foreach (var entity in entities) {
                var parameters = ParametersFrom(info, entity);

                using (var command = CommandFor(name, (TConnection) connection, parameters)) {
                    if (command.ExecuteNonQuery() <= 0) {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Removes an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool Delete<TEntity>(IEntityInfo info, TEntity entity, DbConnection connection) where TEntity : class {
            var parameters = new[] {PrimaryKeyParameterOf(info, entity)};
            using (var command = CommandFor(DeleteCommandText(info, entity), (TConnection) connection, parameters)) {
                return command.ExecuteNonQuery() > 0;
            }
        }


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities, DbConnection connection) where TEntity : class {
            var name = DeleteCommandText(info, entities);

            foreach (var entity in entities) {
                var parameters = new[] {PrimaryKeyParameterOf(info, entity)};

                using (var command = CommandFor(name, (TConnection) connection, parameters)) {
                    if (command.ExecuteNonQuery() <= 0)
                        return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Get number of rows in <paramref name="reader"/>.
        /// </summary>
        protected virtual int RowsCount(DbDataReader reader) {
            var count = 0;
            while (reader.Read()) {
                count++;
            }
            return count;
        }

        /// <summary>
        /// Returns sequence, containing entities (described by their <paramref name="info"/>) 
        /// from first result set of data reader, that satisfies to passed filtering <paramref name="criteria"/>.
        /// </summary>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        protected virtual IEnumerable<object> GetMainEntities(IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(info, x));
            var connection = GetConnection();
            var behavior = GetBehaviorFor(info);
            try {
                using (var command = CommandFor(ReadCommandText(info, criteria), connection, parameters)) {
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


        /// <summary>
        /// Returns sequence of entities, that satisfies to passed filtering <paramref name="criteria"/>,
        /// including additional result sets, of data reader. 
        /// See <see cref="IncludeChildrenAttribute"/> and <see cref="IIncludeon"/> for more details.
        /// </summary>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        /// <seealso cref="IncludeChildrenAttribute"/>
        /// <seealso cref="IIncludeon"/>
        protected virtual IEnumerable<object> ReadToEnd(IAdoEntityInfo mainObjectInfo, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(mainObjectInfo, x));
            var connection = GetConnection();

            var behavior = GetBehaviorFor(mainObjectInfo);
            try {
                using (var command = CommandFor(ReadCommandText(mainObjectInfo, criteria), connection, parameters)) {
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
                                                Internal.TraceEvent(TraceEventType.Warning,
                                                    $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");
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
                                            Internal.TraceEvent(TraceEventType.Warning,
                                                $"In result set #{index} finded data row of type {info.EntityType}, that doesn't has owner object in result set #1.\nOwner Id is {child.Key}.\nTarget path is '{includeon.Attribute.TargetPath}'.");
                                            return;
                                        }

                                        if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, child.Value))
                                            Internal.TraceEvent(TraceEventType.Warning,
                                                $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");

                                        if (string.IsNullOrWhiteSpace(includeon.OwnerNavigationReferenceName))
                                            return;

                                        foreach (var item in child.Value) {
                                            if (!item.TrySetValueOnPath(includeon.OwnerNavigationReferenceName, mainObject))
                                                Internal.TraceEvent(TraceEventType.Warning,
                                                    $"Can't set property {includeon.OwnerNavigationReferenceName} from '{info.EntityType}' context. This should be reference to owner object ({mainObject})");
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

        /// <summary>
        /// Returns one single entity, by reading it's data from <paramref name="reader"/>.
        /// It uses <paramref name="includeon"/> if it was specified, otherwise - it uses <paramref name="mainEntityInfo"/>.
        /// It also return identifier of entity in <paramref name="keyId"/> outer parameter.
        /// </summary>
        /// <param name="reader">Reader contained entities data.</param>
        /// <param name="mainEntityInfo">Info about entity. Used if <paramref name="includeon"/> is not specified (equals to null).</param>
        /// <param name="includeon">Info about includeon. Can be unspecified. See <see cref="IncludeChildrenAttribute"/> for more details.</param>
        /// <param name="keyId">
        /// Identifier of returned entity. 
        /// If <paramref name="includeon"/> parameter unspecified, or represents single field includeon - <paramref name="keyId"/> will contain primary key of returned entity.
        /// If <paramref name="includeon"/> specified and represents sum-collection of main entity (<see cref="IIncludeon.IsCollection"/>) 
        ///     - <paramref name="keyId"/> will contain key primary key of main entity (not the key of returned one). 
        /// </param>
        /// <returns>Main entity (when <paramref name="includeon"/> unspecified) or entity described by <paramref name="includeon"/>.</returns>
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

        /// <summary>
        /// Checks, is property of entity represents it's primary key.
        /// </summary>
        protected abstract bool IsPrimaryKey([NotNull] string propertyName, [NotNull] IAdoEntityInfo mainEntityInfo, [CanBeNull] Includeon includeon);

        /// <summary>
        /// Checks, is property represend foreign key to main entity from the included.
        /// </summary>
        protected virtual bool IsKeyOfMainEntityInForeign([NotNull] string propertyName, [NotNull] IAdoEntityInfo mainEntityInfo, [NotNull] Includeon includeon) {
            return propertyName == includeon.ForeignKeyFromCurrentEntityToMain;
        }
    }
}