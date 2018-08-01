using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Dccelerator.DataAccess.Logging {
    public abstract class LoggerSessionBase : ILoggerSession {
        readonly string _sourceName;
        readonly byte[] _eventId;

        protected LoggerSessionBase(string sourceName, byte[] eventId) {
            _sourceName = sourceName;
            _eventId = eventId;
        }


        protected abstract void Write(TraceEvent traceEvent);
        

        public Task Log(TraceEventType eventType, Func<string> message, params object[] relatedObjects) {
            return Task.Factory.StartNew(() => {
                var traceEvent = new TraceEvent {
                    Id = Guid.NewGuid().ToByteArray(),
                    EventId = _eventId,
                    Type = eventType,
                    Message = message(),
                    //RelatedObjects = relatedObjects,
                    TimeCreated = DateTime.UtcNow,
                    Source = _sourceName
                };

                Write(traceEvent);
            });
        }


        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        #endregion
    }
}