using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dccelerator.DataAccess.MongoDb.Implementation {
    public class NotScheduledMDbTransaction : IDataTransaction {
        readonly IDataManagerMDbFactory _dataManagerMDbFactory;
        readonly TraceSource _trace = new TraceSource("Dccelerator.DataAccess.MongoDb");

        readonly ConcurrentQueue<TransactionElement> _elements = new ConcurrentQueue<TransactionElement>();


        readonly object _lock = new object();
        bool _isCommited;


        public NotScheduledMDbTransaction(IDataManagerMDbFactory dataManagerMDbFactory) {
            _dataManagerMDbFactory = dataManagerMDbFactory;
        }


        public void Dispose() {
            Commit();
        }


        public void Insert<TEntity>(TEntity entity) where TEntity : class {
            _elements.Enqueue(new TransactionElement() {
                ActionType = ActionType.Insert,
                Entity = entity,
                Info = _dataManagerMDbFactory.MDbInfoAbout<TEntity>()
            });
        }


        public void InsertMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            foreach (var entity in entities) {
                _elements.Enqueue(new TransactionElement() {
                    ActionType = ActionType.Insert,
                    Entity = entity,
                    Info = _dataManagerMDbFactory.MDbInfoAbout<TEntity>()
                });
            }
        }


        public void Update<TEntity>(TEntity entity) where TEntity : class {
            _elements.Enqueue(new TransactionElement() {
                ActionType = ActionType.Update,
                Entity = entity,
                Info = _dataManagerMDbFactory.MDbInfoAbout<TEntity>()
            });
        }


        public void UpdateMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            foreach (var entity in entities) {
                _elements.Enqueue(new TransactionElement() {
                    ActionType = ActionType.Update,
                    Entity = entity,
                    Info = _dataManagerMDbFactory.MDbInfoAbout<TEntity>()
                });
            }
        }


        public void Delete<TEntity>(TEntity entity) where TEntity : class {
            _elements.Enqueue(new TransactionElement() {
                ActionType = ActionType.Delete,
                Entity = entity,
                Info = _dataManagerMDbFactory.MDbInfoAbout<TEntity>()
            });
        }


        public void DeleteMany<TEntity>(IEnumerable<TEntity> entities) where TEntity : class {
            foreach (var entity in entities) {
                _elements.Enqueue(new TransactionElement() {
                    ActionType = ActionType.Delete,
                    Entity = entity,
                    Info = _dataManagerMDbFactory.MDbInfoAbout<TEntity>()
                });
            }
        }


        public bool Commit() {
            return Commit(out string error);
        }


        public bool Commit(out string error) {
            error = null;

            if (_isCommited)
                return true;

            lock (_lock) {
                if (_isCommited)
                    return true;

                _isCommited = true;
                try {

                    return _dataManagerMDbFactory.Repository().PerformInTransaction(_elements.Select(x => x.Info).ToList(), _elements);
                }
                catch (Exception e) {
                    error = e.ToString();
                    _trace.TraceEvent(TraceEventType.Critical, 0, $"Error in commit: {e}");
                    return false;
                }
            }
        }


        public bool IsCommited => _isCommited;
    }


    public enum ActionType {
        Insert,
        Update,
        Delete
    }


    public class TransactionElement {
        internal ActionType ActionType { get; set; }

        public IMdbEntityInfo Info { get; set; }

        public object Entity { get; set; }
    }
}