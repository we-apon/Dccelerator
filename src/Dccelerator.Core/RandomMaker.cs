using System;
using System.Collections.Generic;
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
                TrySetRandomValue(entity, prop, includeGuids ?? false);
            }

            actions.Perform(x => x(entity)).ToEnd();
            return entity;
        }



        public static object Make(Type type, bool includeGuids = false) {
            var props = type.GetProperties().Where(x => x.CanWrite).ToArray();
            var entity = Activator.CreateInstance(type);
            foreach (var prop in props) {
                TrySetRandomValue(entity, prop, includeGuids);
            }
            return entity;
        }


        /// <summary>
        /// Makes an entity of <paramref name="type"/> and fills it's public properties with random values.
        /// If public property is not an primitive value - it recursively fills properties of that type, and so on..
        /// </summary>
        /// <param name="type">Type of generated object</param>
        /// <param name="includeGuids">Is it need to generate <see cref="Guid"/>s?</param>
        /// <param name="maxDepth">Maximum depth on generated complex objects</param>
        /// <param name="depth">Current depth of generated object. Used for returning from infinitive loop.</param>
        /// <returns></returns>
        public static object MakeRecursive(Type type, bool includeGuids = false, int maxDepth = 7, int depth = 0) {
            if (depth > maxDepth)
                return null;

            var props = type.GetProperties().Where(x => x.CanWrite).ToArray();

            object entity;
            try {
                entity = Activator.CreateInstance(type);
            }
            catch {
                return null;
            }

            var innerObjectProps = props.Where(prop => !TrySetRandomValue(entity, prop, includeGuids)).ToList();

            foreach (var prop in innerObjectProps) {
                try {
                    var item = MakeRecursive(prop.PropertyType, includeGuids, maxDepth, depth + 1);
                    if (item != null)
                        prop.SetValue(entity, item, null);
                }
                catch {
                    /*do_nothing()*/
                }
            }
            
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
            length = length ?? StaticRandom.Next(4, 32);
            var minChar = minCharIdx ?? 0;
            var maxChar = maxCharIdx ?? 0;

            var builder = new StringBuilder(length.Value);
            var idx = 0;
            while (idx++ < length) {
                char next;

                if (includeDigits && StaticRandom.Next().IsOdd())
                    next = (char) StaticRandom.Next(48, 57);
                else {
                    do {
                        next = (char) StaticRandom.Next(minChar, maxChar);
                    } while (!char.IsLetter(next));
                }

                builder.Append(StaticRandom.Next().IsOdd() ? next : char.ToLower(next));
            }
            return builder.ToString();
        }


        /// <summary>
        /// Generates random array of <see cref="byte"/>.
        /// </summary>
        /// <param name="length">Length of array. If not specified - random length from 9999 to 999999 will be used.</param>
        
        public static byte[] MakeByteArray(int? length = null) {
            length = length ?? StaticRandom.Next(9999, 999999);
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
            return (9999999999 - StaticRandom.Next(1, 999999999)).ToString(CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Makes an random string, that contains only digits.
        /// </summary>
        /// <param name="length">Length or string. Default is '3 to 10'</param>
        
        public static string MakeNumber(int? length = null) {
            length = length ?? StaticRandom.Next(3, 10);

            var builder = new StringBuilder(length.Value).Append(StaticRandom.Next(1, 9));
            var idx = 1;
            while (idx++ < length) {
                builder.Append(StaticRandom.Next(0, 9));
            }
            return builder.ToString();
        }


        #region private

        static readonly Type _stringType = typeof (string);
        static readonly Type _guidType = typeof (Guid);
        static readonly Type _intType = typeof (int);
        static readonly Type _doubleType = typeof (double);
        static readonly Type _boolType = typeof (bool);
        static readonly Type _dateTimeType = typeof (DateTime);
        static readonly Type _decimalType = typeof (decimal);


        static bool TrySetRandomValue(object entity, PropertyInfo prop, bool includeGuids) {
            var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            object value = null;

            if (_stringType.IsAssignableFrom(type))
                value = prop.Name.ToLowerInvariant().Contains("phone") ? MakePhone() : MakeString();
            else if (_doubleType.IsAssignableFrom(type))
                value = StaticRandom.NextDouble()*StaticRandom.Next(1, int.MaxValue);
            else if (_decimalType.IsAssignableFrom(type))
                value = (decimal) (StaticRandom.NextDouble()*StaticRandom.Next(1, int.MaxValue));
            else if (_intType.IsAssignableFrom(type))
                value = StaticRandom.Next();
            else if (_boolType.IsAssignableFrom(type))
                value = StaticRandom.Next().IsOdd();
            else if (_dateTimeType.IsAssignableFrom(type))
                value = DateTime.UtcNow.AddMinutes(-(StaticRandom.Next(0, 95*365*24*60)));
            else if (includeGuids && _guidType.IsAssignableFrom(type))
                value = Guid.NewGuid();

            return value != null && entity.TrySet(prop.Name, value);
        }

        #endregion

    }
}