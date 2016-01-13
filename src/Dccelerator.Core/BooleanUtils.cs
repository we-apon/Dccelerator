namespace Dccelerator
{
    /// <summary>
    /// Declarative extensions for converting <see cref="int"/> to <see cref="bool"/>.
    /// </summary>
    public static class BooleanUtils
    {

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this int number) {
            return number > 0;
        }


        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> is not <see langword="null"/> and greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this int? number) {
            return number != null && number.Value > 0;
        }




        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this int number) {
            return number < 0
                ? (bool?) null
                : number > 0;
        }
            
            
        
        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this int? number) {
            return number?.AsNullableBoolean();
        }

    }


}