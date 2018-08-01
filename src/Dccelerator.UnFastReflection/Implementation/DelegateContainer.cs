using System.Reflection;


namespace Dccelerator.UnFastReflection
{

    public abstract class DelegateContainer<TDelegate> : MethodDelegateBase
        where TDelegate: class 
    {
        protected DelegateContainer(MethodInfo method) : base(method) {}

        TDelegate _delegate;

        public TDelegate Delegate => _delegate ?? (_delegate = GetDelegate<TDelegate>());

    }

}