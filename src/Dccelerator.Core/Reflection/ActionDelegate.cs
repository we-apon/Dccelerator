using System;
using System.Reflection;


namespace Dccelerator.Reflection
{
    public class ActionDelegate<TContext> : DelegateContainer<Action<TContext>>
    {
        public ActionDelegate(MethodInfo method) : base(method) {}


        public bool TryInvoke(TContext context) {
            try {
                Delegate(context);
                return true;
            }
            catch (Exception) { //todo: add logging
                return false;
            }
        }
    }


    public class ActionDelegate<TContext, TP1> : DelegateContainer<Action<TContext, TP1>>
    {
        public ActionDelegate(MethodInfo method) : base(method) {}


        public bool TryInvoke(TContext context, TP1 p1) {
            try {
                Delegate(context, p1);
                return true;
            }
            catch (Exception) { //todo: add logging
                return false;
            }
        }
    }

}