using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace Dccelerator.DataAccess.Ado.Implementation {
    /// <summary>
    /// An realization of <see cref="IDataTransaction"/> that is not deferring anything.
    /// Once methods like <see cref="Insert{TEntity}"/> or <see cref="UpdateMany{TEntity}"/> called - in just doing it and no waits for anyone.
    /// <see cref="Dispose"/> and <see cref="Commit"/> is unused. They're doing nothing.
    /// </summary>
    public abstract class NotScheduledDataTransaction : IDataTransaction {
        readonly IDataManagerAdoFactory _factory;
        readonly IsolationLevel _isolationLevel;

        readonly ConcurrentQueue<Func<DbConnection, bool>> _actions = new ConcurrentQueue<Func<DbConnection, bool>>();
        bool _isCommited;


        protected NotScheduledDataTransaction(IDataManagerAdoFactory factory, IsolationLevel isolationLevel) {
            _factory = factory;
            _isolationLevel = isolationLevel;
        }


        const int DefaultRetryCount = 6;


        protected virtual TResult RetryOnDeadlock<TResult>(Func<TResult> func, int retryCount = DefaultRetryCount) {
            var attemptNumber = 1;
            while (true) {
                try {
                    return func();
                }
                catch (Exception exception) {
                    Infrastructure.Internal.TraceEvent(TraceEventType.Warning, $"On attempt count #{attemptNumber} gaived sql exception:\n{exception}");
                    if (!IsDeadlockException(exception) || (attemptNumber++ > retryCount))
                        throw;
                }
            }
        }



        protected abstract bool IsDeadlockException(Exception exception);




        /// <summary>
        /// Inserts <paramref name="entities"/> into database.
        /// </summary>
        public void InsertMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(connection => info.Repository.InsertMany(info, entities, connection));
        }


        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");


            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(connection => info.Repository.Insert(info, entity, connection));
        }


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        public void Update<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(connection => info.Repository.Update(info, entity, connection));
        }


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(connection => info.Repository.UpdateMany(info, entities, connection));
        }


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(connection => info.Repository.Delete(info, entity, connection));
        }


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(connection => info.Repository.DeleteMany(info, entities, connection));
        }



        protected virtual System.Data.IsolationLevel GetDataIsolationLevel(IsolationLevel localIsolationLevel) {
            switch (localIsolationLevel) {
                    case IsolationLevel.Serializable: return System.Data.IsolationLevel.Serializable;
                    case IsolationLevel.RepeatableRead: return System.Data.IsolationLevel.RepeatableRead;
                    case IsolationLevel.ReadCommitted: return System.Data.IsolationLevel.ReadCommitted;
                    case IsolationLevel.ReadUncommitted: return System.Data.IsolationLevel.ReadUncommitted;
                    case IsolationLevel.Snapshot: return System.Data.IsolationLevel.Snapshot;
                    case IsolationLevel.Chaos: return System.Data.IsolationLevel.Chaos;
                    case IsolationLevel.Unspecified: return System.Data.IsolationLevel.Unspecified;

                default:
                    System.Data.IsolationLevel level;
                    return Enum.TryParse(localIsolationLevel.ToString("G"), out level)
                        ? level
                        : System.Data.IsolationLevel.Unspecified;
            }
        }


        /// <summary>
        /// Doing nothing.
        /// </summary>
        /// <returns>Always returns <see langword="true"/>.</returns>
        public bool Commit() {
            if (_isCommited)
                return true;

            _isCommited = true;

            return RetryOnDeadlock(() => {
                var queue = _actions.ToArray();

                try {
                    using (var connection = _factory.AdoNetRepository().GetConnection()) {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction(GetDataIsolationLevel(_isolationLevel))) {
                            foreach (var action in queue) {
                                if (!action(connection)) {
                                    _isCommited = false;
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            transaction.Commit();
                            return true;
                        }

                    }
                }
                catch (Exception e) {
                    new TraceSource("Dccelerator.DataAccess").TraceEvent(TraceEventType.Critical, 0, e.ToString());
                    return false;
                }
            });
        }

        



        /// <summary>
        /// Returns state of current transaction
        /// </summary>
        public bool IsCommited => _isCommited;


        #region Implementation of IDisposable

        /// <summary>
        /// Doing nothing.
        /// </summary>
        public virtual void Dispose() {
            Commit();
        }


        #endregion
    }
}