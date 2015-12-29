using System;
using System.Collections;


namespace Dccelerator.Reflection
{
    /// <summary>
    /// Declarative extensions usable for interacting with <see cref="Type"/>s.
    /// </summary>
    public static class TypeUtils
    {
        static Type _typeInfo = typeof (TypeInfo<>);

        /// <summary>
        /// Checks, is <paramref name="type"/> implements <see cref="IEnumerable"/> <see langword="interface"/>, but is not a <see cref="string"/>.
        /// </summary>
        /// <seealso cref="EnumerableUtils.IsStrongEnumerable"/>
        /// <seealso cref="EnumerableUtils.AsStrongEnumerable"/>
        public static bool IsStrongEnumerable(this Type type) {
            return TypeCache.IsStrongEnumerable(type);
        }
        

    }
}