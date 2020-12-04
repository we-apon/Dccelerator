﻿using System.Diagnostics;

#if  NETSTANDARD1_3
using System.Reflection;
#endif

namespace Dccelerator {

    static class Log {
#if NETSTANDARD1_3
        static readonly TraceSource _trace = new TraceSource(typeof(Log).GetTypeInfo().Assembly.GetName().Name);
#else
        static readonly TraceSource _trace = new TraceSource(typeof(Log).Assembly.GetName().Name);
#endif


        internal static void TraceEvent(TraceEventType eventType, string message) {
#if NETSTANDARD1_3
            /* do nothing */
#else
            _trace.Switch = new SourceSwitch(nameof(Log)) {
                Level = SourceLevels.All
            };
            _trace.Listeners.Remove("Default");
            _trace.Listeners.Add(new TextWriterTraceListener("Dccelerator.Inner.log") {
                Filter = new EventTypeFilter(SourceLevels.All)
            });
#endif

            _trace.TraceEvent(eventType, 0, message);
        }
    }

}