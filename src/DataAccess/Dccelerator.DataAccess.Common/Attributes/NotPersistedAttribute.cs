using System;


namespace Dccelerator.DataAccess
{
    /// <summary>
    /// Tells what property is not stored in Database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotPersistedAttribute : Attribute { }
}