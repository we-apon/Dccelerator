using System.Reflection;


namespace Dccelerator.Reflection.Abstract
{

    public abstract class DelegateContainer<TDelegate> : MethodDelegateBase//, IDelegateContainer<TDelegate> 
        where TDelegate: class 
    {
        protected DelegateContainer(MethodInfo method) : base(method) {}

        TDelegate _delegate;

        public TDelegate Delegate => _delegate ?? (_delegate = GetLambda<TDelegate>());



/*
        public bool TryInvoke<TContext, TOut>(TContext context, out TOut result, params object[] args) {
            throw new NotImplementedException();
        }



        public bool TryInvoke<TContext, TOut>(TContext context, out TOut result) {
            try {
                var func = Delegate.CastTo<Func<TContext, TOut>>();
                result = func(context);
                return true;
            }
            catch (Exception) { //todo: add logging
                result = default(TOut);
                return false;
            }
        }


        public bool TryInvoke<TContext>(TContext context, params object[] args) {
            switch (args.Length) {
                case 0:
                    var f = Delegate.CastTo<Action<TContext>>();
                    f(context);
                    return true;

                case 1:
                    var f1 = Delegate.CastTo<Action<TContext, object>>();
                    f1(context, args[0]);
                    return true;
            }
            throw new NotImplementedException();
        }
*/

    }

}