using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dccelerator.Reflection;


namespace Dccelerator
{
    /// <summary>
    /// Declarative extensions for interacting with collections
    /// </summary>
    public static class EnumerableUtils
    {


        public static TValue AggregateBranch<TContext, TValue>(this TContext context, Func<TContext, TContext> getChild, Func<TContext, TValue, TValue> aggregate) {
            var result = default (TValue);
            var current = context;

            while (current != null) {
                result = aggregate(current, result);
                current = getChild(current);
            }
            return result;
        }


        public static string AggregateExceptionMessage(this Exception exception) {
            var builder = new StringBuilder(exception.Message);

            while ((exception = exception.InnerException) != null) {
                builder.Append("\n\n").Append(exception.Message);
            }

            return builder.ToString();
        }


        /// <summary>
        /// Returns <paramref name="collection"/> in <see cref="Random"/> order.
        /// Returned collection are <see langword="yield"/>ed.
        /// </summary>
        /// <param name="collection">Any generic collection.</param>
        
        public static  IOrderedEnumerable<T> Shuffle<T>( this IEnumerable<T> collection) {
            return collection.OrderBy(x => StaticRandom.Next());
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
            return typeof(T).IsArray || collection.GetType().IsArray;
        }



        /// <summary>
        /// Adds all <paramref name="items"/> to <see cref="IList{T}"/> and then returns it.
        /// </summary>
        public static IList<T> AddRange<T>( this IList<T> collection, IEnumerable<T> items) {
            if (collection is List<T> list) {
                list.AddRange(items);
                return list;
            }

            bool isArray;
            if ((isArray = collection.IsArray()) || collection is ReadOnlyCollection<T>) {
                var error = $"{collection} is an {(isArray ? "array" : "read only collection")} so it can't be modified!";
                Internal.TraceEvent(TraceEventType.Warning, error);
                throw new InvalidOperationException(error);
            }

            foreach (var item in items) {
                collection.Add(item);
            }

            return collection;
        }

        

        /// <summary>
        /// Counts elements in <paramref name="enumerable"/>.
        /// If <paramref name="enumerable"/> <see langword="is"/> <see cref="ICollection"/> - method just will returns it's <see cref="ICollection.Count"/>.
        /// If <paramref name="enumerable"/> isn't <see cref="ICollection"/> - <paramref name="enumerable"/> will be actually enumerated to end.
        /// </summary>
        /// <param name="enumerable">Any collection</param>
        public static int Count( this IEnumerable enumerable) {
            if (enumerable is ICollection collection)
                return collection.Count;

            var count = 0;
            var enumerator = enumerable.GetEnumerator();
            try {
                while (enumerator.MoveNext())
                    count++;
            }
            catch {
                //do_nothing();
            }

            return count;
        }


        /// <summary>
        /// Checks, is <paramref name="enumerable"/> has any elements.
        /// </summary>
        /// <param name="enumerable">An collection</param>
        public static bool HasAny(this IEnumerable enumerable) {
            if (enumerable is ICollection collection)
                return collection.Count != 0;
            
            var enumerator = enumerable.GetEnumerator();
            try {
                return enumerator.MoveNext();
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Warning, e.ToString());
                return false;
            }
        }



        /// <summary>
        /// Checks, is elements in <paramref name="enumerable"/> are distinct.
        /// </summary>
        /// <param name="enumerable">An collection.</param>
        public static bool IsDistinct( this IEnumerable enumerable) {
            var enumerator = enumerable.GetEnumerator();
            var hashSet = new HashSet<object>();
            while (enumerator.MoveNext()) {
                if (!hashSet.Add(enumerator.Current))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Returns <paramref name="value"/> as <see cref="IEnumerable"/>, if it is an collection, and not an string.
        /// Otherwise return <see langword="null"/>.
        /// </summary>
        /// <seealso cref="TypeCache.IsAnCollection"/>
        public static IEnumerable AsAnCollection(this object value) {
            if (value is string)
                return null;

            return value is IEnumerable enumerable ? enumerable : null;
        }


        /// <summary>
        /// Applies specified <paramref name="action"/> on every item from <paramref name="collection"/> in yielded loop.
        /// </summary>
        public static IEnumerable<T> Perform<T>( this IEnumerable<T> collection,  Action<T> action) {
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
            using (var enumerator = collection.GetEnumerator())
                while (enumerator.MoveNext()) { /*do_nothing()*/ }
        }


        /// <summary>
        /// Returns distinct subset of <paramref name="collection"/> by customizable <paramref name="criterion"/>.
        /// This method uses yield return.
        /// </summary>
        
        public static IEnumerable<TItem> DistinctBy<TItem, TKey>(this IEnumerable<TItem> collection, Func<TItem, TKey> criterion) {
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
        /// Returns <see langword="true"/> if <paramref name="enumerable"/> is null, or has not items.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) {
            if (enumerable == null)
                return true;

            var str = enumerable as string;
            if (str != null)
                return string.IsNullOrEmpty(str);

            if (enumerable is ICollection collection)
                return collection.Count == 0;
            
            return !enumerable.Any();
        }

    }
}