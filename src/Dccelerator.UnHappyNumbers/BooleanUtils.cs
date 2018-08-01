namespace Dccelerator.UnHappyNumbers
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
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this uint number) {
            return number > 0;
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this long number) {
            return number > 0;
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this ulong number) {
            return number > 0;
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this byte number) {
            return number > 0;
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this sbyte number) {
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
        /// Returns <see langword="true"/>, if <paramref name="number"/> is not <see langword="null"/> and greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this uint? number) {
            return number != null && number.Value > 0;
        }


        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> is not <see langword="null"/> and greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this long? number) {
            return number != null && number.Value > 0;
        }


        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> is not <see langword="null"/> and greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this ulong? number) {
            return number != null && number.Value > 0;
        }


        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> is not <see langword="null"/> and greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this byte? number) {
            return number != null && number.Value > 0;
        }


        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> is not <see langword="null"/> and greather than 0.
        /// Otherwise returns <see langword="false"/>
        /// </summary>
        public static bool AsBoolean(this sbyte? number) {
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
        public static bool? AsNullableBoolean(this uint number) {
            return number < 0
                ? (bool?) null
                : number > 0;
        }
            
        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this long number) {
            return number < 0
                ? (bool?) null
                : number > 0;
        }
            
        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this ulong number) {
            return number < 0
                ? (bool?) null
                : number > 0;
        }
            
        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this byte number) {
            return number < 0
                ? (bool?) null
                : number > 0;
        }
            
        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this sbyte number) {
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

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this uint? number) {
            return number?.AsNullableBoolean();
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this long? number) {
            return number?.AsNullableBoolean();
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this ulong? number) {
            return number?.AsNullableBoolean();
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this byte? number) {
            return number?.AsNullableBoolean();
        }

        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="number"/> greather than 0.
        /// Returns <see langword="false"/>, if <paramref name="number"/> equal 0.
        /// Otherwise returns <see langword="null"/>.
        /// </summary>
        public static bool? AsNullableBoolean(this sbyte? number) {
            return number?.AsNullableBoolean();
        }

    }


}