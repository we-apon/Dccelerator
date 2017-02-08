namespace Dccelerator.Reflection {

    public interface IFunctionDelegate<in TContext, TOut> {
        bool TryInvoke(TContext context, out TOut result);
    }

    public interface IFunctionDelegate<in TContext, in TP1, TOut> {
        bool TryInvoke(TContext context, TP1 p1, out TOut result);
    }

    public interface IFunctionDelegate<in TContext, in TP1, in TP2, TOut> {
        bool TryInvoke(TContext context, TP1 p1, TP2 p2, out TOut result);
    }

}