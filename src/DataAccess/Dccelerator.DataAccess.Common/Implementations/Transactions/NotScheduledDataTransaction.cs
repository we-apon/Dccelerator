using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Dccelerator.DataAccess.Infrastructure;
using System.Linq;
using System.Transactions;


namespace Dccelerator.DataAccess.Implementations.Transactions {


    class SimpleScheduledTransaction : NotScheduledDataTransaction {
        private readonly ITransactionScheduler _scheduler;


        public SimpleScheduledTransaction(ITransactionScheduler scheduler, IDataManagerFactory factory, IsolationLevel isolationLevel) : base(factory, isolationLevel) {
            _scheduler = scheduler;
        }


        #region Overrides of NotScheduledDataTransaction

        /// <summary>
        /// Doing nothing.
        /// </summary>
        public override void Dispose() {
            if (IsCommited)
                return;

            _scheduler.Append(this); //todo: this is will be work, but that is bad way.
        }

        #endregion
    }



    /// <summary>
    /// An realization of <see cref="IDataTransaction"/> that is not deferring anything.
    /// Once methods like <see cref="Insert{TEntity}"/> or <see cref="UpdateMany{TEntity}"/> called - in just doing it and no waits for anyone.
    /// <see cref="Dispose"/> and <see cref="Commit"/> is unused. They're doing nothing.
    /// </summary>
    class NotScheduledDataTransaction : IDataTransaction {
        private readonly IDataManagerFactory _factory;
        private readonly IsolationLevel _isolationLevel;
        private readonly object _lock = new object();

        private readonly ConcurrentQueue<Func<IDataManagerFactory, bool>> _actions = new ConcurrentQueue<Func<IDataManagerFactory, bool>>();
        private bool _isCommited;

        public NotScheduledDataTransaction(IDataManagerFactory factory, IsolationLevel isolationLevel) {
            _factory = factory;
            _isolationLevel = isolationLevel;
        }


        private const int DefaultRetryCount = 6;

        private const int DeadlockErrorNumber = 1205;
        private const int LockingErrorNumber = 1222;
        private const int UpdateConflictErrorNumber = 3960;


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






        /// <summary>
        /// Inserts <paramref name="entities"/> into database.
        /// </summary>
        public void InsertMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = ConfigurationOf<TEntity>.Info;
            var repository = info.RealRepository;
            var name = info.EntityName;
            _actions.Enqueue(factory => repository.InsertMany(name, entities));
        }


        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = ConfigurationOf<TEntity>.Info;
            var repository = info.RealRepository;
            var name = info.EntityName;
            _actions.Enqueue(factory => repository.Insert(name, entity));
        }


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        public void Update<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = ConfigurationOf<TEntity>.Info;
            var repository = info.RealRepository;
            var name = info.EntityName;
            _actions.Enqueue(factory => repository.Update(name, entity));
        }


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = ConfigurationOf<TEntity>.Info;
            var repository = info.RealRepository;
            var name = info.EntityName;
            _actions.Enqueue(factory => repository.UpdateMany(name, entities));
        }


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = ConfigurationOf<TEntity>.Info;
            var repository = info.RealRepository;
            var name = info.EntityName;
            _actions.Enqueue(factory => repository.Delete(name, entity));
        }


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = ConfigurationOf<TEntity>.Info;
            var repository = info.RealRepository;
            var name = info.EntityName;
            _actions.Enqueue(factory => repository.DeleteMany(name, entities));
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

                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = _isolationLevel})) {
                    foreach (var action in queue) {
                        if (!action(_factory)) {
                            _isCommited = false;
                            return false;
                        }
                    }

                    scope.Complete();
                    return true;
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