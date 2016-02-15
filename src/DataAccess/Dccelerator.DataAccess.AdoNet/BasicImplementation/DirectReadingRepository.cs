using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace Dccelerator.DataAccess.Ado.BasicImplementation {


    public abstract class DirectReadingRepository : IReadingRepository {

        protected virtual int DeadLockRetryCount => 6;

        protected TResult RetryOnDeadlock<TResult>(Func<TResult> func) {
            var attemptNumber = 1;
            while (true) {
                try {
                    return func();
                }
                catch (Exception exception) {
                    Infrastructure.Internal.TraceEvent(TraceEventType.Critical, $"On attempt coun #{attemptNumber} gaived exception:\n{exception}");

                    if (!IsDeadlock(exception) || (attemptNumber++ > DeadLockRetryCount))
                        throw;
                }
            }
        }


        protected abstract bool IsDeadlock(Exception exception);


        protected virtual string IdentityStringOf(string entityName,  IEnumerable<IDataCriterion> criteria) {
            var builder = new StringBuilder(entityName);
            foreach (var criterion in criteria) {
                builder.Append(criterion.Name).Append(criterion.Value);
            }
            return builder.ToString();
        }


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