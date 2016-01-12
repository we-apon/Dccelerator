namespace Dccelerator
{
    namespace NumberExtensions
    {
        /// <summary>
        /// Declarative extension of numeric types
        /// </summary>
        public static class NumberExtensions
        {
            /// <summary>
            /// Returns <see langword="true"/> if <paramref name="number"/> is odd.
            /// </summary>
            /// <seealso cref="IsEven"/>
            public static bool IsOdd(this int number) {
                return number%2 != 0;
            }

            /// <summary>
            /// Returns <see langword="true"/> if <paramref name="number"/> is even.
            /// </summary>
            /// <seealso cref="IsOdd"/>
            public static bool IsEven(this int number) {
                return number%2 == 0;
            }
        }
    }
}