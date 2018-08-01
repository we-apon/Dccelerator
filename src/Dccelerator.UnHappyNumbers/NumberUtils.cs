namespace Dccelerator.UnHappyNumbers {


    /// <summary>
    /// Declarative extension of numeric types
    /// </summary>
    public static class NumberExtensions {


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
        /// </summary>
        /// <seealso cref="IsEven(int)"/>
        public static bool IsOdd(this int number) {
            return number % 2 != 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
        /// </summary>
        /// <seealso cref="IsEven(uint)"/>
        public static bool IsOdd(this uint number) {
            return number % 2 != 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
        /// </summary>
        /// <seealso cref="IsEven(long)"/>
        public static bool IsOdd(this long number) {
            return number % 2 != 0;
        }



        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
        /// </summary>
        /// <seealso cref="IsEven(ulong)"/>
        public static bool IsOdd(this ulong number) {
            return number % 2 != 0;
        }



        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
        /// </summary>
        /// <seealso cref="IsEven(byte)"/>
        public static bool IsOdd(this byte number) {
            return number % 2 != 0;
        }



        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
        /// </summary>
        /// <seealso cref="IsEven(sbyte)"/>
        public static bool IsOdd(this sbyte number) {
            return number % 2 != 0;
        }




        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is even.
        /// </summary>
        /// <seealso cref="IsOdd(int)"/>
        public static bool IsEven(this int number) {
            return number % 2 == 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is even.
        /// </summary>
        /// <seealso cref="IsOdd(uint)"/>
        public static bool IsEven(this uint number) {
            return number % 2 == 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is even.
        /// </summary>
        /// <seealso cref="IsOdd(long)"/>
        public static bool IsEven(this long number) {
            return number % 2 == 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is even.
        /// </summary>
        /// <seealso cref="IsOdd(ulong)"/>
        public static bool IsEven(this ulong number) {
            return number % 2 == 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is even.
        /// </summary>
        /// <seealso cref="IsOdd(byte)"/>
        public static bool IsEven(this byte number) {
            return number % 2 == 0;
        }


        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="number"/> is even.
        /// </summary>
        /// <seealso cref="IsOdd(sbyte)"/>
        public static bool IsEven(this sbyte number) {
            return number % 2 == 0;
        }
    }
}