using System.Diagnostics;

#if PORTABLE
namespace System.Diagnostics {
    enum TraceEventType {
        Critical = 1,
        Error = 2,
        Warning = 4,
        Information = 8,
        Verbose = 16,
        [EditorBrowsable(EditorBrowsableState.Advanced)] Start = 256,
        [EditorBrowsable(EditorBrowsableState.Advanced)] Stop = 512,
        [EditorBrowsable(EditorBrowsableState.Advanced)] Suspend = 1024,
        [EditorBrowsable(EditorBrowsableState.Advanced)] Resume = 2048,
        [EditorBrowsable(EditorBrowsableState.Advanced)] Transfer = 4096,
    }
}


#endif

namespace Dccelerator {


    static class Internal {
#if !PORTABLE

        static readonly TraceSource _trace = new TraceSource("Dccelerator");

        internal static void TraceEvent(TraceEventType eventType,  string message) {
            _trace.TraceEvent(eventType, 0, message);
        }
#else


        internal static void TraceEvent(TraceEventType eventType,  string message) {
            Debug.WriteLine($"[{eventType.ToString("G")}]\t{message}");
        }

#endif
    }

}