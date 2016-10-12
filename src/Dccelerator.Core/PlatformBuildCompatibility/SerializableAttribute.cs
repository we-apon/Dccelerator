#if NET_CORE_APP || NET_STANDARD

// ReSharper disable CheckNamespace

namespace System
{
    /// <summary>
    /// Indicates that a class can be serialized. This class cannot be inherited.
    /// </summary>
    /// <filterpriority>1</filterpriority>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    public sealed class SerializableAttribute : Attribute
    {

    }
}

#endif