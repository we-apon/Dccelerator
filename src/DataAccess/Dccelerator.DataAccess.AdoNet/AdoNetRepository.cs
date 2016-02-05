using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dccelerator.Reflection;


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

                return ParameterWith(x.Key, x.Value, value);
            });
        }
        


        /// <summary>
        /// Returns reader that can be used to get some data by <paramref name="entityName"/>, filtering it by <paramref name="criteria"/>.
        /// </summary>
        /// <param name="entityName">Database-specific name of some entity</param>
        /// <param name="criteria">Filtering criteria</param>
        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.To(info);
            }
        }


        public bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.Read();
            }
        }


        public IEnumerable<object> ReadColumn(int columnIdx, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.SelectColumn(columnIdx);
            }
        }


        public int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.RowsCount();
            }
        }


        /// <summary>
        /// Inserts an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool Insert<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            var parameters = ParametersFrom(info, entity);

            using (var connection = GetConnection()) {
                using (var command = CommandFor(NameOfInsertProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }



        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfInsertProcedureFor(info.EntityName);
            using (var connection = GetConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(info, entity);

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
        public virtual bool Update<T>(IEntityInfo info, T entity) where T : class {
            var parameters = ParametersFrom(info, entity);

            using (var connection = GetConnection()) {
                using (var command = CommandFor(NameOfUpdateProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfUpdateProcedureFor(info.EntityName);
            using (var connection = GetConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(info, entity);

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
        public virtual bool Delete<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            var parameters = new [] { PrimaryKeyParameterOf(info, entity) };

            using (var connection = GetConnection()) {
                using (var command = CommandFor(NameOfDeleteProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfDeleteProcedureFor(info.EntityName);
            using (var connection = GetConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    
                    var parameters = new[] { PrimaryKeyParameterOf(info, entity)};

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