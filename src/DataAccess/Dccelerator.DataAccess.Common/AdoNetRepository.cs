using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess {
    

    /// <summary>
    /// Am <see cref="IDataAccessRepository"/> what defines special names of CRUD-operation stored procedures.
    /// </summary>
    /// <seealso cref="NameOfReadProcedureFor"/>
    /// <seealso cref="NameOfInsertProcedureFor"/>
    /// <seealso cref="NameOfUpdateProcedureFor"/>
    /// <seealso cref="NameOfDeleteProcedureFor"/>
    public abstract class AdoNetRepository<TCommand, TParameter, TConnection> : IDataAccessRepository
        where TCommand : DbCommand
        where TParameter: DbParameter
        where TConnection : DbConnection {


        protected abstract TConnection InstantinateConnection();

        protected abstract TParameter ParameterWith(string name, Type type, object value);

        protected abstract TCommand CommandFor(string commandText, TConnection connection, IEnumerable<TParameter> parameters, CommandType type = CommandType.StoredProcedure);


        
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


        protected virtual IEnumerable<TParameter> ParametersFrom<TEntity>(TEntity entity) where TEntity : class {
            var parameters = ConfigurationOf<TEntity>.Info.PersistedFields.Select(x => {
                object value;
                if (!TypeManipulator<TEntity>.TryGetNestedProperty(entity, x.Item1, out value))
                    throw new InvalidOperationException($"Entity of type {entity.GetType()} should contain property '{x.Item1}', " +
                                                        $"but in some reason value or that property could not be getted.");

                return ParameterWith(x.Item1, x.Item2, value);
            });
            return parameters;
        }



        /// <summary>
        /// Returns reader that can be used to get some data by <paramref name="entityName"/>, filtering it by <paramref name="criteria"/>.
        /// </summary>
        /// <param name="entityName">Database-specific name of some entity</param>
        /// <param name="criteria">Filtering criteria</param>
        /// <param name="reader">An data reader</param>
        /// <returns>Connection of <paramref name="reader"/>. Reader and connection will be disposed just after all requested information are readed.</returns>
        public virtual DbConnection Read(string entityName, ICollection<IDataCriterion> criteria, out DbDataReader reader) {

            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            var connection = InstantinateConnection();

            using (var command = CommandFor(NameOfReadProcedureFor(entityName), connection, parameters)) {               
                connection.Open();
                reader = command.ExecuteReader();
                return connection;
            }
        }


        /// <summary>
        /// Inserts an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool Insert<TEntity>(string entityName, TEntity entity) where TEntity : class {
            var parameters = ParametersFrom(entity);

            using (var connection = InstantinateConnection()) {
                using (var command = CommandFor(NameOfInsertProcedureFor(entityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }



        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool InsertMany<TEntity>(string entityName, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfInsertProcedureFor(entityName);
            using (var connection = InstantinateConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(entity);

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
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
        public virtual bool Update<T>(string entityName, T entity) where T : class {
            var parameters = ParametersFrom(entity);

            using (var connection = InstantinateConnection()) {
                using (var command = CommandFor(NameOfUpdateProcedureFor(entityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool UpdateMany<TEntity>(string entityName, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfUpdateProcedureFor(entityName);
            using (var connection = InstantinateConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(entity);

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
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
        public virtual bool Delete<TEntity>(string entityName, TEntity entity) where TEntity : class {
            var info = ConfigurationOf<TEntity>.Info;
            if (info.KeyId == null)
                throw new InvalidOperationException($"In order to delete entities of type {TypeManipulator<TEntity>.Type}, you must specify {nameof(EntityAttribute.IdProperty)} in {nameof(EntityAttribute)} " +
                                                            $"on entity {info.EntityType}, or use identifier in property named 'Id' or '{TypeManipulator<TEntity>.Type.Name + "Id"}'.");

            object id;
            if (!TypeManipulator<TEntity>.TryGetNestedProperty(entity, info.KeyId.Name, out id))
                throw new InvalidOperationException($"Entity of type {entity.GetType()} should contain property '{info.KeyId.Name}', " +
                                                    $"but in some reason value or that property could not be getted.");

            var parameters = new [] {ParameterWith(info.KeyId.Name, info.KeyId.PropertyType, id)};

            using (var connection = InstantinateConnection()) {
                using (var command = CommandFor(NameOfDeleteProcedureFor(entityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool DeleteMany<TEntity>(string entityName, IEnumerable<TEntity> entities) where TEntity : class {
                    var info = ConfigurationOf<TEntity>.Info;
                    if (info.KeyId == null)
                        throw new InvalidOperationException($"In order to delete entities of type {TypeManipulator<TEntity>.Type}, you must specify {nameof(EntityAttribute.IdProperty)} in {nameof(EntityAttribute)} " +
                                                                    $"on entity {info.EntityType}, or use identifier in property named 'Id' or '{TypeManipulator<TEntity>.Type.Name + "Id"}'.");

            var name = NameOfDeleteProcedureFor(entityName);
            using (var connection = InstantinateConnection()) {
                connection.Open();

                foreach (var entity in entities) {

                    object id;
                    if (!TypeManipulator<TEntity>.TryGetNestedProperty(entity, info.KeyId.Name, out id))
                        throw new InvalidOperationException($"Entity of type {entity.GetType()} should contain property '{info.KeyId.Name}', " +
                                                            $"but in some reason value or that property could not be getted.");

                    var parameters = new[] {ParameterWith(info.KeyId.Name, info.KeyId.PropertyType, id)};



                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}