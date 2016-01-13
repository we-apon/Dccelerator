using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dccelerator.Reflection;


namespace Dccelerator
{
    /// <summary>
    /// Declarative extensions for interacting with collections
    /// </summary>
    public static class EnumerableUtils
    {
        static readonly Random _random = new Random();

        /// <summary>
        /// Returns <paramref name="collection"/> in <see cref="Random"/> order.
        /// Returned collection are <see langword="yield"/>ed.
        /// </summary>
        /// <param name="collection">Any generic collection.</param>
        
        public static  IOrderedEnumerable<T> Shuffle<T>( this IEnumerable<T> collection) {
            return collection.OrderBy(x => _random.Next());
        }


        /// <summary>
        /// Checks, is <paramref name="collection"/> actually an <see cref="Array"/>.
        /// </summary>
        /// <param name="collection">Any collection, even not generic.</param>
        public static bool IsArray( this IEnumerable collection) {
            return collection.GetType().IsArray;
        }


        /// <summary>
        /// Checks, is <paramref name="collection"/> actually an <see cref="Array"/>.
        /// </summary>
        /// <param name="collection">Any collection, even not generic.</param>
        public static bool IsArray<T>(this T collection) where T : IEnumerable {
            return typeof(T).IsArray;
        }



        /// <summary>
        /// Adds all <paramref name="items"/> to <see cref="IList{T}"/> and then returns it.
        /// </summary>
        
        public static IList<T> Add<T>( this IList<T> list,  params T[] items) {
            foreach (var item in items) {
                list.Add(item);
            }
            return list;
        }


        /// <summary>
        /// Checks, is <paramref name="enumerable"/> <see cref="ICollection"/>.
        /// It's actually returns <code>enumerable is ICollection</code>.
        /// </summary>
        /// <param name="enumerable">An enumerable</param>
        public static bool IsCollection( this IEnumerable enumerable) {
            return enumerable is ICollection;
        }


        /// <summary>
        /// Counts elements in <paramref name="enumerable"/>.
        /// If <paramref name="enumerable"/> <see langword="is"/> <see cref="ICollection"/> - method just will returns it's <see cref="ICollection.Count"/>.
        /// If <paramref name="enumerable"/> isn't <see cref="ICollection"/> - <paramref name="enumerable"/> will be actually enumerated to end.
        /// </summary>
        /// <param name="enumerable">Any collection</param>
        public static int Count( this IEnumerable enumerable) {
            var collection = enumerable as ICollection;
            if (collection != null)
                return collection.Count;

            var count = 0;
            var enumerator = enumerable.GetEnumerator();
            try {
                while (enumerator.MoveNext())
                    count++;
            }
            catch (Exception) {
                //do_nothing();
            }

            return count;
        }


        /// <summary>
        /// Checks, is elements in <paramref name="enumerable"/> are distinct.
        /// </summary>
        /// <param name="enumerable">An collection.</param>
        public static bool IsDistinct( this IEnumerable enumerable) {
            var collection = enumerable as ICollection;
            if (collection != null) {
                var hash = new HashSet<object>(collection.Cast<object>());
                return hash.Count == collection.Count;
            }

            var enumerator = enumerable.GetEnumerator();
            var count = 0;
            var hashSet = new HashSet<object>();
            while (enumerator.MoveNext()) {
                hashSet.Add(enumerator.Current);
                count++;
            }
            return hashSet.Count == count;
        }


        /// <summary>
        /// Returns <paramref name="value"/> as <see cref="IEnumerable"/>, if it is an collection, and not an string.
        /// Otherwise return <see langword="null"/>.
        /// </summary>
        /// <seealso cref="TypeCache.IsAnCollection"/>
        public static IEnumerable AsAnCollection( this object value,  Type valueType = null) {
            if (value == null)
                return null;

            return (valueType ?? value.GetType()).IsAnCollection()
                ? (IEnumerable) value
                : null;
        }


        /// <summary>
        /// Applies specified <paramref name="action"/> on every item from <paramref name="collection"/> in yielded loop.
        /// </summary>
        /// <seealso cref="Map{T}"/>
        public static IEnumerable<T> Perform<T>( this IEnumerable<T> collection,  Action<T> action) {
            foreach (var item in collection) {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Same as <see cref="Perform{T}"/> method, applies specified <paramref name="action"/> on every item from <paramref name="collection"/> in yielded loop.
        /// In most languages such method called 'map', so.
        /// </summary>
        /// <seealso cref="Perform{T}"/>
        public static IEnumerable<T> Map<T>( this IEnumerable<T> collection,  Action<T> action) {
            foreach (var item in collection) {
                action(item);
                yield return item;
            }
        }


        /// <summary>
        /// Enumerates <paramref name="collection"/> to it's end.
        /// </summary>
        /// <example>
        /// <code>
        /// var i = 0;
        /// var array = new int[5];
        /// array.Perform(x => x = i++).ToEnd();
        /// </code>
        /// </example>
        public static void ToEnd<T>( this IEnumerable<T> collection) {
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext()) { /*do_nothing()*/ }
        }


        /// <summary>
        /// Returns distinct subset of <paramref name="collection"/> by customizable <paramref name="criterion"/>.
        /// This method uses yield return.
        /// </summary>
        
        public static IEnumerable<TItem> DistinctBy<TItem, TKey>( this IEnumerable<TItem> collection,  Func<TItem, TKey> criterion) {
            var set = new HashSet<TKey>();

            foreach (var item in collection) {
                var key = criterion(item);
                if (set.Contains(key))
                    continue;

                set.Add(key);
                yield return item;
            }
        }



        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="collection"/> is null, or has not items.
        /// </summary>
        public static bool IsNullOrEmpty<T>( this IEnumerable<T> collection) {
            return collection == null || !collection.Any();
        }

    }
}