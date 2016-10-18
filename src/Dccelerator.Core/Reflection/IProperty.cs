using System.Reflection;

namespace Dccelerator.Reflection {
    public interface IProperty {
        PropertyInfo Info { get; }


        bool TryGetValue(object context, out object value);

        bool TrySetValue(object context, object value);
    }

    public interface IProperty<TValue> : IProperty {
        bool TryGetValue(object context, out TValue value);
    }

    public interface IProperty<in TContext, TValue> : IProperty<TValue> {
        bool TryGetValue(TContext context, out TValue value);
    }
}