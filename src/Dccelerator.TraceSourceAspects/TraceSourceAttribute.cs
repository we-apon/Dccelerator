using System;


namespace Dccelerator.TraceSourceAttributes {

    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class TraceSourceAttribute : TraceSourceAttributeBase {

    }
     
}