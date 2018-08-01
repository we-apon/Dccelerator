using System.Diagnostics;


namespace Dccelerator.UnExpressAssertion {
    public delegate void LogAssertionDelegate(TraceEventType eventType, string message, params object[] relatedObjects);
}