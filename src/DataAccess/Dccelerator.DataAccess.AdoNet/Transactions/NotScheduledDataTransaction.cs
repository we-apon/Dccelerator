using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;


namespace Dccelerator.DataAccess.Ado.Transactions {
    /// <summary>
    /// An realization of <see cref="IDataTransaction"/> that is not deferring anything.
    /// Once methods like <see cref="Insert{TEntity}"/> or <see cref="UpdateMany{TEntity}"/> called - in just doing it and no waits for anyone.
    /// <see cref="Dispose"/> and <see cref="Commit"/> is unused. They're doing nothing.
    /// </summary>
    public abstract class NotScheduledDataTransaction : IDataTransaction {
        readonly IDataManagerAdoFactory _factory;
        readonly IsolationLevel _isolationLevel;
        readonly object _lock = new object();

        readonly ConcurrentQueue<Func<IDataManagerFactory, bool>> _actions = new ConcurrentQueue<Func<IDataManagerFactory, bool>>();
        bool _isCommited;


        public NotScheduledDataTransaction(IDataManagerAdoFactory factory, IsolationLevel isolationLevel) {
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
                    Internal.TraceEvent(TraceEventType.Warning, $"On attempt count #{attemptNumber} gaived sql exception:\n{exception}");
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
            _actions.Enqueue(factory => info.Repository.InsertMany(info, entities));
        }


        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");


            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(factory => info.Repository.Insert(info, entity));
        }


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        public void Update<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(factory => info.Repository.Update(info, entity));
        }


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(factory => info.Repository.UpdateMany(info, entities));
        }


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(factory => info.Repository.Delete(info, entity));
        }


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            if (_isCommited)
                throw new InvalidOperationException($"Transaction is locked. It means that it already commited or rolled back.");

            var info = _factory.AdoInfoAbout<TEntity>();
            _actions.Enqueue(factory => info.Repository.DeleteMany(info, entities));
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

                using (var scope = BeginTransactionScope(_isolationLevel)) {
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


        protected abstract ISpecificTransactionScope BeginTransactionScope(IsolationLevel isolationLevel);





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