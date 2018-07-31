namespace Dccelerator.Reflection
{

    public interface IDelegateContainer<out TDelegate> where TDelegate : class
    {
        string Name { get; }

        TDelegate Delegate { get; }

        bool TryInvoke<TContext, TOut>(TContext context, out TOut result, params object[] args);

        bool TryInvoke<TContext, TOut>(TContext context, out TOut result);
    }



    
}