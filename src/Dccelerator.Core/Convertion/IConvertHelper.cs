using System;


namespace Dccelerator.Convertion.Abstract
{
    /// <summary>
    /// Interface of some kind of helper class, that can be used with <see cref="ConvertibleToAttribute"/> and <see cref="ConvertibleFromAttribute"/>
    /// </summary>
    public interface IConvertHelper
    {
        /// <summary>
        /// Converts <paramref name="entity"/> to <paramref name="targetType"/>
        /// </summary>
        /// <param name="targetType">An type</param>
        /// <param name="entity">An entity</param>
        object ConvertTo( Type targetType,  object entity);
    }
}