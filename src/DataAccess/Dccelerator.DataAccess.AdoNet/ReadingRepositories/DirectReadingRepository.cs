using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Dccelerator.DataAccess.Ado.Infrastructure;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado.ReadingRepositories {


    public abstract class DirectReadingRepository : IInternalReadingRepository {

/*
        public class EntityConfig {
            public EntityInfo Info;
            public IAdoNetRepository Repository;
            public string[] ColumnNames;
        }

        readonly ConcurrentDictionary<Type, EntityConfig> _configs = new ConcurrentDictionary<Type, EntityConfig>();
*/


        protected virtual int DeadLockRetryCount => 6;

        protected TResult RetryOnDeadlock<TResult>(Func<TResult> func) {
            var attemptNumber = 1;
            while (true) {
                try {
                    return func();
                }
                catch (Exception exception) { //todo: make it work!
                    Internal.TraceEvent(TraceEventType.Critical, $"On attempt coun #{attemptNumber} gaived exception:\n{exception}");

                    if (!IsDeadlock(exception) || (attemptNumber++ > DeadLockRetryCount))
                        throw;
                }
            }
        }


        protected abstract bool IsDeadlock(Exception exception);

/*

        protected EntityConfig ConfigOf(Type entityType) {
            EntityConfig config;
            if (_configs.TryGetValue(entityType, out config))
                return config;

            config = new EntityConfig {
                Info = new ConfigurationOfEntity(entityType).Info
            };

            config.Repository = (IAdoNetRepository) Activator.CreateInstance(config.Info.RealRepositoryType);

            if (!_configs.TryAdd(entityType, config))
                config = _configs[entityType];

            return config;
        }

        */
        protected virtual string IdentityStringOf(string entityName,  IEnumerable<IDataCriterion> criteria) {
            var builder = new StringBuilder(entityName);
            foreach (var criterion in criteria) {
                builder.Append(criterion.Name).Append(criterion.Value);
            }
            return builder.ToString();
        }

/*
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
        */
        

        #region Implementation of IInternalReadingRepository

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        public virtual IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return RetryOnDeadlock(() => ((IAdoEntityInfo)info).Repository.Read(info, criteria));
        }


        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        public virtual bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return RetryOnDeadlock(() => ((IAdoEntityInfo)info).Repository.Any(info, criteria));
        }


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        public virtual IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return RetryOnDeadlock(() => ((IAdoEntityInfo)info).Repository.ReadColumn(columnName, info, criteria));
        }


        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        public virtual int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return RetryOnDeadlock(() => ((IAdoEntityInfo)info).Repository.CountOf(info, criteria));
        }

        #endregion
    }
}