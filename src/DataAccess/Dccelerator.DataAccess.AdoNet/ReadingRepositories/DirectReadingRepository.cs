using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado.ReadingRepositories {


    class DirectReadingRepository : IInternalReadingRepository {

        public class EntityConfig {
            public EntityInfo Info;
            public IDataAccessRepository Repository;
            public string[] ColumnNames;
        }

        readonly ConcurrentDictionary<Type, EntityConfig> _configs = new ConcurrentDictionary<Type, EntityConfig>();



        const int DefaultRetryCount = 6;


        protected TResult RetryOnDeadlock<TResult>(Func<TResult> func, int retryCount = DefaultRetryCount) {
            var attemptNumber = 1;
            while (true) {
                try {
                    return func();
                }
                catch (Exception e) { //todo: make it work!
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

            return RetryOnDeadlock(() => config.Repository.Read(entityName, criteria, config.Info));
        }


        public virtual bool Any( string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            var repository = ConfigOf(entityType).Repository;

            return RetryOnDeadlock(() => repository.Any(entityName, criteria));
        }

        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        
        public virtual IEnumerable<object> ReadColumn( string columnName,  string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            var config = ConfigOf(entityType);

            throw new NotImplementedException();

            //return RetryOnDeadlock(() => config.Repository.ReadColumn(Array.IndexOf(ColumnNamesFrom(config, reader), columnName), entityName, criteria));
        }


        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public virtual int CountOf( string entityName, Type entityType,  ICollection<IDataCriterion> criteria) {
            return RetryOnDeadlock(() => ConfigOf(entityType).Repository.CountOf(entityName, criteria));
        }
    }
}