using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class NotScheduledBDbTransaction : IDataTransaction {

        enum ActionType {
            Insert,
            Update,
            Delete
        }

        class TransactionElement {
            public ActionType ActionType { get; set; }

            public BDbEntityInfo Info { get; set; }

            public object Entity { get; set; }
        }

        
        readonly ConcurrentQueue<TransactionElement> _elements = new ConcurrentQueue<TransactionElement>();

        readonly object _lock = new object();




        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            throw new NotImplementedException();
        }

        #endregion


        #region Implementation of IDataTransaction

        /// <summary>
        /// Inserts <paramref name="entity"/> into database.
        /// </summary>
        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            var element = new TransactionElement {
                ActionType = ActionType.Insert,
                Entity = entity,
                Info = new BDbEntityInfo() //todo something with it
            };
            

            _elements.Enqueue(element);
        }


        /// <summary>
        /// Inserts <paramref name="entities"/> into database.
        /// </summary>
        public void InsertMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        public void Update<TEntity>(TEntity entity) where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Updates <paramref name="entities"/> in database.
        /// </summary>
        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Removes <paramref name="entity"/> from database.
        /// </summary>
        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Removes <paramref name="entities"/> from database.
        /// </summary>
        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Immidiatelly executes all prepared actions of this transaction.
        /// If this method is not called, but transaction are disposed - all prepared actions will be performed later, in some scheduler.
        /// </summary>
        /// <returns>Result of performed transaction.</returns>
        public bool Commit() {
            lock (_lock) {
                if (_isCommited)
                    return true;

                _isCommited = true;


                foreach (var transactionElement in _elements) {
                    if (transactionElement.ActionType != ActionType.Insert) {
                        if (!transactionElement.Info.Repository.Insert(transactionElement.Entity, transactionElement.Info.Type.Name, transactionElement.Info.Mappings.Values))
                            return false;

                        continue;
                    }

                    throw new NotImplementedException();
                }

                return true;
            }
        }


        /// <summary>
        /// Returns state of current transaction
        /// </summary>
        public bool IsCommited => _isCommited;


        bool _isCommited;

        #endregion
    }
}