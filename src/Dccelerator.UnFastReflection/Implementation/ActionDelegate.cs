using System;
using System.Diagnostics;
using System.Reflection;


namespace Dccelerator.UnFastReflection
{
    public class ActionDelegate<TContext> : DelegateContainer<Action<TContext>>, IActionDelegate<TContext> {
        public ActionDelegate(MethodInfo method) : base(method) {}


        public bool TryInvoke(TContext context) {
            try {
                Delegate(context);
                return true;
            }
            catch (Exception e) {
                Log.TraceEvent(TraceEventType.Error, e.ToString());
                return false;
            }
        }
    }



    public class ActionDelegate<TContext, TP1> : DelegateContainer<Action<TContext, TP1>>, IActionDelegate<TContext, TP1> {
        public ActionDelegate(MethodInfo method) : base(method) {}


        public bool TryInvoke(TContext context, TP1 p1) {
            try {
                Delegate(context, p1);
                return true;
            }
            catch (Exception e) {
                Log.TraceEvent(TraceEventType.Error, e.ToString());
                return false;
            }
        }
    }

}