using System.Collections.Concurrent;
using Dccelerator.DataAccess.Logging;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbContinuousLoggerSession : LoggerSessionBase {
        readonly IDataManager _bdbDataManager;
        readonly ConcurrentQueue<TraceEvent> _queue = new ConcurrentQueue<TraceEvent>();

        public BDbContinuousLoggerSession(IDataManager bdbDataManager, string sourceName, byte[] eventId) : base(sourceName, eventId) {
            _bdbDataManager = bdbDataManager;
        }


        #region Overrides of LoggerSessionBase

        protected override void Write(TraceEvent traceEvent) {
            _queue.Enqueue(traceEvent);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose() {
            using (var transaction = _bdbDataManager.BeginTransaction()) {
                transaction.InsertMany(_queue);
                transaction.Commit();
            }
        }

        #endregion
    }



    public class BDbImmidiateLoggerSession : LoggerSessionBase {
        readonly IDataManager _bdbDataManager;
        public BDbImmidiateLoggerSession(IDataManager bdbDataManager, string sourceName, byte[] eventId) : base(sourceName, eventId) {
            _bdbDataManager = bdbDataManager;
        }


        #region Overrides of LoggerSessionBase

        protected override void Write(TraceEvent traceEvent) {
            using (var transaction = _bdbDataManager.BeginTransaction()) {
                transaction.Insert(traceEvent);
                transaction.Commit();
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose() {
            /*do_nothing()*/
        }

        #endregion
    }
}