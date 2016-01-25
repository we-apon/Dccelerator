using System.Diagnostics;


namespace Dccelerator.DataAccess {
    static class Internal {
        static readonly TraceSource _trace = new TraceSource("Dccelerator.DataAccess");


        internal static void TraceEvent(TraceEventType eventType,  string message) {
            _trace.TraceEvent(eventType, 0, message);
        }
    }
}