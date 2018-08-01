

namespace Dccelerator.UnSmartConvertion
{
    /// <summary>
    /// Declarative extensions for objects type casting.
    /// </summary>
    public static class CastingUtils
    {
        /// <summary>
        /// Returns <paramref name="objct"/> as <typeparamref name="T"/>, if <paramref name="objct"/> <see langword="is"/> <typeparamref name="T"/>.
        /// Otherwise returns <see langword="default"/> <typeparamref name="T"/>.
        /// </summary>
        public static T SafeCastTo<T>( this object objct) {
            return (objct is T variable) ? variable : default(T);
        }


        /// <summary>
        /// Returns true, if <paramref name="objct"/> <see langword="is"/> <typeparamref name="T"/>.
        /// </summary>
        public static bool TryCastTo<T>( this object objct, out T result) {
            if (objct is T variable) {
                result = variable;
                return true;
            }

            result = default(T);
            return false;
        }


        /// <summary>
        /// Returns <see langword="null"/>, if <paramref name="obj"/> == null. 
        /// Otherwise returns <paramref name="obj"/><c>.ToString()</c>.
        /// </summary>
        public static string ToStringOrNull<T>(this T obj) {
            return obj != null ? obj.ToString() : null;
        }


        /// <summary>
        /// Returns null, if <paramref name="obj"/> == null. 
        /// Otherwise returns <see cref="string.Empty"/>
        /// </summary>
        public static string ToStringOrEmpty<T>(this T obj) {
            return obj != null ? obj.ToString() : string.Empty;
        }
    }
}