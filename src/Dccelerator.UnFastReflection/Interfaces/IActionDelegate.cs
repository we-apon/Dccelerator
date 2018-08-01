using System;


namespace Dccelerator.UnFastReflection {

    public interface IActionDelegate<in TContext> {

        bool TryInvoke(TContext context);
    }


    public interface IActionDelegate<in TContext, in TP1> {

        Action<TContext, TP1> Delegate { get; }

        bool TryInvoke(TContext context, TP1 p1);
    }

}