namespace Dccelerator.Reflection {

    public interface IActionDelegate<in TContext> {
        bool TryInvoke(TContext context);
    }


    public interface IActionDelegate<in TContext, in TP1> {
        bool TryInvoke(TContext context, TP1 p1);
    }

}