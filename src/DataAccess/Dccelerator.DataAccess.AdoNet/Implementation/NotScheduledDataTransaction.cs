using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Dccelerator.DataAccess.Ado.Implementation {
    /// <summary>
    /// An realization of <see cref="IDataTransaction"/> that is not deferring anything.
    /// Once methods like <see cref="Insert{TEntity}"/> or <see cref="UpdateMany{TEntity}"/> called - in just doing it and no waits for anyone.
    /// <see cref="Dispose"/> and <see cref="Commit"/> is unused. They're doing nothing.
    /// </summary>
    public abstract class NotScheduledDataTransaction : IDataTransaction {
        readonly IDataManagerAdoFactory _factory;
        readonly IsolationLevel _isolationLevel;

        readonly ConcurrentQueue<Func<DbActionArgs, bool>> _actions = new ConcurrentQueue<Func<DbActionArgs, bool>>();
        bool _isCommited;


        protected NotScheduledDataTransaction(IDataManagerAdoFactory factory, IsolationLevel isolationLevel) {
            _factory = factory;
            _isolationLevel = isolationLevel;
        }


        const int DefaultRetryCount = 6;


        protected virtual bool RetryOnDeadlock(Action action, out string error, int retryCount = DefaultRetryCount) {
            var attemptNumber = 1;
            while (true) {
                try {
                    action();
                    error = null;
                    return true;
                }
                catch (Exception exception) {
                    error = $"On attempt count #{attemptNumber} gaived sql exception:\n{exception}";

                    if (!IsDeadlockException(exception) || (attemptNumber++ > retryCount)) {
                        Infrastructure.Internal.TraceEvent(TraceEventType.Critical, error);
                        return false;
                    }

                    Infrastructure.Internal.TraceEvent(TraceEventType.Warning, error);
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
            _actions.Enqueue(args => info.Repository.InsertMany(info, entities, args));
        }


        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");


            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(args => info.Repository.Insert(info, entity, args));
        }


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        public void Update<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(args => info.Repository.Update(info, entity, args));
        }


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(args => info.Repository.UpdateMany(info, entities, args));
        }


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(args => info.Repository.Delete(info, entity, args));
        }


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(args => info.Repository.DeleteMany(info, entities, args));
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
            string unusedError;
            return Commit(out unusedError);
        }


        /// <summary>
        /// Immidiatelly executes all prepared actions of this transaction.
        /// If this method is not called, but transaction are disposed - all prepared actions will be performed later, in some scheduler.
        /// </summary>
        /// <param name="error">Output parameter containing error message, if some error occured</param>
        /// <returns>Result of performed transaction.</returns>
        public bool Commit(out string error) {
            error = null;

            if (_isCommited)
                return true;

            _isCommited = true;

            return RetryOnDeadlock(() => {
                var queue = _actions.ToArray();

                using (var connection = _factory.AdoNetRepository().GetConnection()) {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction(GetDataIsolationLevel(_isolationLevel))) {
                        var args = new DbActionArgs(connection, transaction);

                        foreach (var action in queue) {
                            if (!action(args)) {
                                _isCommited = false;
                                transaction.Rollback();
                                throw new Exception("Unknown error occured. Transaction is rolled back.");
                            }
                        }

                        transaction.Commit();
                    }
                }
            }, out error);
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