using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Dccelerator.Logging {
    public interface ILoggerSession : IDisposable {
        Task Log(TraceEventType eventType, Func<string> message, params object[] relatedObjects);
    }
}