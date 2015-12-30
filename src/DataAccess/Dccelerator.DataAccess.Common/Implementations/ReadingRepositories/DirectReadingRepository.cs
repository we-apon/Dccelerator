using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using Dccelerator.DataAccess.Infrastructure;
using System.Linq;

namespace Dccelerator.DataAccess.Implementations.ReadingRepositories {


    class DirectReadingRepository : IInternalReadingRepository {

        public class EntityConfig {
            public EntityInfo Info;
            public IDataAccessRepository Repository;
            public string[] ColumnNames;
        }

        readonly ConcurrentDictionary<Type, EntityConfig> _configs = new ConcurrentDictionary<Type, EntityConfig>();


        const int DefaultRetryCount = 6;

        const int DeadlockErrorNumber = 1205;
        const int LockingErrorNumber = 1222;
        const int UpdateConflictErrorNumber = 3960;


        protected TResult RetryOnDeadlock<TResult>(Func<TResult> func, int retryCount = DefaultRetryCount) {
            var attemptNumber = 1;
            while (true) {
                try {
                    return func();
                }
                catch (SqlException exception) {
                    Internal.TraceEvent(TraceEventType.Warning, $"On attempt count #{attemptNumber} gaived sql exception:\n{exception}");
                    if (!exception.Errors.Cast<SqlError>().Any(error =>
                        (error.Number == DeadlockErrorNumber) ||
                        (error.Number == LockingErrorNumber) ||
                        (error.Number == UpdateConflictErrorNumber))) {
                        Internal.TraceEvent(TraceEventType.Critical, $"Sql exception has only bad errors: {exception.Errors.Cast<SqlError>().Aggregate(string.Empty, (s, error) => $"[ErrorNumber #{error.Number} {error.Message}] ")}");
                        throw;
                    }
                    if (attemptNumber == retryCount + 1) {
                        Internal.TraceEvent(TraceEventType.Critical, "Attempt count exceeded retry count");
                        throw;
                    }
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Critical, $"On attempt coun #{attemptNumber} gaived exception:\n{e}");
                    throw;
                }
                attemptNumber++;
            }
        }


        protected EntityConfig ConfigOf(Type entityType) {
            EntityConfig config;
            if (_configs.TryGetValue(entityType, out config))
                return config;

            config = new EntityConfig {
                Info = new ConfigurationOfEntity(entityType).Info
            };

            config.Repository = (IDataAccessRepository) Activator.CreateInstance(config.Info.RealRepositoryType);

            if (!_configs.TryAdd(entityType, config))
                config = _configs[entityType];

            return config;
        }

        
        protected virtual string IdentityStringOf(string entityName,  IEnumerable<IDataCriterion> criteria) {
            var builder = new StringBuilder(entityName);
            foreach (var criterion in criteria) {
                builder.Append(criterion.Name).Append(criterion.Value);
            }
            return builder.ToString();
        }


        string[] ColumnNamesFrom(EntityConfig config, DbDataReader reader) {
            if (config.ColumnNames != null)
                return config.ColumnNames;

            lock (config) {
                if (config.ColumnNames == null) {
                    config.ColumnNames = reader.GetSchemaTable()?.Rows.Cast<DataRow>().Select(x => (string) x[0]).ToArray();
                }
            }
            return config.ColumnNames;
        }


        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        
        public virtual IEnumerable<object> Read( string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            var config = ConfigOf(entityType);


            return RetryOnDeadlock(() => {
                DbDataReader reader;
                var connection = config.Repository.Read(entityName, criteria, out reader);
                //var columnNames = ColumnNamesFrom(config, reader);
                return reader.To(connection, config.Info);
            });
        }


        public virtual bool Any( string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            var repository = ConfigOf(entityType).Repository;

            return RetryOnDeadlock(() => {
                DbDataReader reader;
                using (repository.Read(entityName, criteria, out reader)) {
                    return reader.Read();
                }
            });
        }


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        
        public virtual IEnumerable<object> ReadColumn( string columnName,  string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            var config = ConfigOf(entityType);

            return RetryOnDeadlock(() => {
                DbDataReader reader;
                var connection = config.Repository.Read(entityName, criteria, out reader);
                return reader.SelectColumn(Array.IndexOf(ColumnNamesFrom(config, reader), columnName), connection);
            });
        }


        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public virtual int CountOf( string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            return RetryOnDeadlock(() => {
                DbDataReader reader;
                var connection = ConfigOf(entityType).Repository.Read(entityName, criteria, out reader);
                return reader.RowsCount(connection);
            });
        }
    }
}