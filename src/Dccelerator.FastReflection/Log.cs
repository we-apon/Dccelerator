using System.Diagnostics;


namespace Dccelerator.Reflection {
    
    static class Log {
        static readonly TraceSource _trace = new TraceSource("Dccelerator.FastReflection");

        internal static void TraceEvent(TraceEventType eventType,  string message) {
            _trace.TraceEvent(eventType, 0, message);
        }
    }

}