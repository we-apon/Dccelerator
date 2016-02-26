using System.Reflection;

namespace Dccelerator.Reflection
{
    public interface IProperty
    {
        PropertyInfo Info { get; }


        bool TryGetValue(object context, out object value);

        bool TrySetValue(object context, object value);
    }
}