
#if DOTNET

// ReSharper disable CheckNamespace
namespace System
{
    /// <summary>
    /// Indicates that a field of a serializable class should not be serialized. This class cannot be inherited.
    /// </summary>
    /// <filterpriority>1</filterpriority>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NonSerializedAttribute : Attribute {}
}

#endif