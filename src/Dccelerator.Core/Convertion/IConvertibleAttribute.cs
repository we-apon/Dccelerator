using System;


namespace Dccelerator.Convertion.Abstract
{
    /// <summary>
    /// Interface of attributes used for customization of conversion with <see cref="SmartConvert"/> utility. 
    /// </summary>
    public interface IConvertibleAttribute
    {
        /// <summary>
        /// <see cref="Type"/> that implemented <see cref="IConvertHelper"/> interface and having an public parameterless constructor
        /// </summary>
        /// <exception cref="InvalidOperationException" accessor="set">should be sub-type of <see cref="IConvertHelper"/></exception>
        
        Type Through { get; }


        /// <summary>
        /// Checks, it <paramref name="targetType"/> and be builded used specified convertion helper.
        /// </summary>
        bool IsUsableFor( Type targetType);

        /// <summary>
        /// Collection of target types, that can be builded <seealso cref="Through"/> specified convertion helper.
        /// </summary>
        
        Type[] TargetTypes { get; }
    }
}