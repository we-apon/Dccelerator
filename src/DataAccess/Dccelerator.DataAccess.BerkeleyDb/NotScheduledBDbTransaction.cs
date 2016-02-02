using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class NotScheduledBDbTransaction : IDataTransaction {
        readonly IDataManagerBDbFactory _factory;


        readonly ConcurrentQueue<TransactionElement> _elements = new ConcurrentQueue<TransactionElement>();


        

        readonly object _lock = new object();


        public NotScheduledBDbTransaction(IDataManagerBDbFactory factory) {
            _factory = factory;
        }

        void AppendTransactionElement<TEntity>(TEntity entity, ActionType actionType) where TEntity : class {
            var info = _factory.InfoAbout<TEntity>();
            _elements.Enqueue(new TransactionElement {
                ActionType = actionType,
                Entity = entity,
                Info = info
            });
        }

        

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Commit();
        }

        #endregion


        #region Implementation of IDataTransaction

        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            AppendTransactionElement(entity, ActionType.Insert);
        }




        /// <summary>
        /// Inserts <paramref name="entities"/> into database.
        /// </summary>
        public void InsertMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            foreach (var entity in entities) {
                AppendTransactionElement(entity, ActionType.Insert);
            }
        }


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        public void Update<TEntity>(TEntity entity) where TEntity : class {
            AppendTransactionElement(entity, ActionType.Update);
        }


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            foreach (var entity in entities) {
                AppendTransactionElement(entity, ActionType.Update);
            }
        }


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            AppendTransactionElement(entity, ActionType.Delete);
        }


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            foreach (var entity in entities) {
                AppendTransactionElement(entity, ActionType.Delete);
            }
        }


        /// <summary>
        /// Immidiatelly executes all prepared actions of this transaction.
        /// If this method is not called, but transaction are disposed - all prepared actions will be performed later, in some scheduler.
        /// </summary>
        /// <returns>Result of performed transaction.</returns>
        public bool Commit() {
            if (_isCommited)
                return true;

            lock (_lock) {
                if (_isCommited)
                    return true;

                _isCommited = true;

                return _factory.Repository().PerformInTransaction(_elements);
            }
        }


        /// <summary>
        /// Returns state of current transaction
        /// </summary>
        public bool IsCommited => _isCommited;


        bool _isCommited;

        #endregion
    }


    public enum ActionType {
        Insert,
        Update,
        Delete
    }


    public class TransactionElement {
        internal ActionType ActionType { get; set; }

        public IBDbEntityInfo Info { get; set; }

        public object Entity { get; set; }
    }
}