using System;
using PostSharp.Serialization;


namespace Dccelerator.UnAuthomatedTracing {

    /// <summary>
    /// <para xml:lang="en"></para>
    /// <para xml:lang="ru">
    /// PostSharp-атрибут для трассировки выполнения методов. 
    /// Трассировка выполняется фреймворком System.Diagnostics.TraceSource, встронным в .Net Framework.
    /// Кстати, для знакомства с System.Diagnostics.TraceSource хорошо подходит документация расширяюшего его фрейворка - <see hrev="https://essentialdiagnostics.codeplex.com/">Essential.Diagnostics</see>.
    /// </para>
    /// </summary>
#if NETCOREAPP2_2
    [PSerializable]
#else
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Assembly)]
    public sealed class TraceSourceAttribute : TraceSourceAttributeBase {

    }
     
}