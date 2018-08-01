using System.Reflection;


namespace Dccelerator.UnFastReflection {
    public interface IProperty {
        PropertyInfo Info { get; }

        object GetValue(object context);
        bool TryGetValue(object context, out object value);


        void SetValue(object context, object value);
        bool TrySetValue(object context, object value);
    }

    public interface IProperty<TValue> : IProperty {
        bool TryGetValue(object context, out TValue value);
    }

    public interface IProperty<in TContext, TValue> : IProperty<TValue> {

        IFunctionDelegate<TContext, TValue> Getter { get; }

        IActionDelegate<TContext, TValue> Setter { get; }

        bool TryGetValue(TContext context, out TValue value);

        bool TrySetValue(TContext context, TValue value);
    }
}