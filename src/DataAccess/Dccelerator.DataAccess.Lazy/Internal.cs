using System.Diagnostics;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Lazy {
    static class Internal
    {
        static readonly TraceSource _trace = new TraceSource("Dccelerator.DataAccess");


        internal static void TraceEvent(TraceEventType eventType, [NotNull] string message)
        {
            _trace.TraceEvent(eventType, 0, message);
        }
    }
}