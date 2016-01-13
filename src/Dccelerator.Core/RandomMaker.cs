using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Dccelerator.NumberExtensions;
using Dccelerator.Reflection;


namespace Dccelerator
{
    /// <summary>
    /// Generates random stuff
    /// </summary>
    public class RandomMaker
    {
        /// <summary>
        /// Makes an entity of <typeparamref name="T"/> and fills it's public properties with random values
        /// </summary>
        public static T Make<T>() where T : new() {
            return Make(false, new Action<T>[0]);
        }


        /// <summary>
        /// Makes an entity of <typeparamref name="T"/> and fills it's public properties with random values
        /// </summary>
        /// <param name="actions">Actions, that will be performed on entity, after random generation</param>
        public static T Make<T>(params Action<T>[] actions) where T : new() {
            return Make(false, actions);
        }


        /// <summary>
        /// Makes an entity of <typeparamref name="T"/> and fills it's public properties with random values
        /// </summary>
        /// <param name="includeGuids">Is it need to generate <see cref="Guid"/>s?</param>
        /// <param name="actions">Actions, that will be performed on entity, after random generation</param>
        public static T Make<T>(bool? includeGuids, params Action<T>[] actions) where T : new() {
            var type = typeof (T);
            var props = type.GetProperties().Where(x => x.CanWrite).ToArray();
            var entity = new T();
            foreach (var prop in props) {
                TrySetRandomValue(entity, prop, includeGuids.GetValueOrDefault());
            }

            actions.Perform(x => x(entity)).ToEnd();
            return entity;
        }


        /// <summary>
        /// Generates random strings
        /// </summary>
        /// <param name="length">Length of string</param>
        /// <param name="minCharIdx">Minimum character index in UTF-8. Default is Russian 'А'</param>
        /// <param name="maxCharIdx">Maximum character index in UTF-8. Default is Russian 'Я'</param>
        /// <param name="includeDigits">Is string may contain digits?</param>
        /// <returns>An random string for passed criteria</returns>
        
        public static string MakeString(int? length = null, short? minCharIdx = 1040, short? maxCharIdx = 1071, bool includeDigits = false) {
            length = length ?? _random.Next(4, 32);
            var minChar = minCharIdx.GetValueOrDefault();
            var maxChar = maxCharIdx.GetValueOrDefault();

            var builder = new StringBuilder(length.GetValueOrDefault());
            var idx = 0;
            while (idx++ < length) {
                char next;

                if (includeDigits && _random.Next().IsOdd())
                    next = (char) _random.Next(48, 57);
                else {
                    do {
                        next = (char) _random.Next(minChar, maxChar);
                    } while (!char.IsLetter(next));
                }

                builder.Append(_random.Next().IsOdd() ? next : char.ToLower(next));
            }
            return builder.ToString();
        }


        /// <summary>
        /// Generates random array of <see cref="byte"/>.
        /// </summary>
        /// <param name="length">Length of array. If not specified - random length from 9999 to 999999 will be used.</param>
        
        public static byte[] MakeByteArray(int? length = null) {
            length = length ?? _random.Next(9999, 999999);
            return MakeString(length)
#if PORTABLE
                .Cast<char>()
#endif
                .Select(x => (byte) (x%255)).ToArray();
        }


        /// <summary>
        ///  Makes an random 10-digit phone number, like that: 9605163162
        /// </summary>
        
        public static string MakePhone() {
            return (9999999999 - _random.Next(1, 999999999)).ToString(CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Makes an random string, that contains only digits.
        /// </summary>
        /// <param name="length">Length or string. Default is '3 to 10'</param>
        
        public static string MakeNumber(int? length = null) {
            length = length ?? _random.Next(3, 10);

            var builder = new StringBuilder(length.GetValueOrDefault()).Append(_random.Next(1, 9));
            var idx = 1;
            while (idx++ < length) {
                builder.Append(_random.Next(0, 9));
            }
            return builder.ToString();
        }


        #region private

        private static readonly Random _random = new Random();
        private static readonly Type _stringType = typeof (string);
        private static readonly Type _guidType = typeof (Guid);
        private static readonly Type _intType = typeof (int);
        private static readonly Type _doubleType = typeof (double);
        private static readonly Type _boolType = typeof (bool);
        private static readonly Type _dateTimeType = typeof (DateTime);
        private static readonly Type _decimalType = typeof (decimal);


        private static void TrySetRandomValue(object entity, PropertyInfo prop, bool includeGuids) {
            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            object value = null;

            if (_stringType.IsAssignableFrom(type))
                value = prop.Name.ToLowerInvariant().Contains("phone") ? MakePhone() : MakeString();
            else if (_doubleType.IsAssignableFrom(type))
                value = _random.NextDouble()*_random.Next(1, int.MaxValue);
            else if (_decimalType.IsAssignableFrom(type))
                value = (decimal) (_random.NextDouble()*_random.Next(1, int.MaxValue));
            else if (_intType.IsAssignableFrom(type))
                value = _random.Next();
            else if (_boolType.IsAssignableFrom(type))
                value = _random.Next().IsOdd();
            else if (_dateTimeType.IsAssignableFrom(type))
                value = DateTime.UtcNow.AddMinutes(-(_random.Next(0, 95*365*24*60)));
            else if (includeGuids && _guidType.IsAssignableFrom(type))
                value = Guid.NewGuid();


            if (value != null)
                prop.SmartSetValue(entity, value);
        }

        #endregion

    }
}