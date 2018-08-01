using System;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace Dccelerator.UnEnumeratedHelpers
{
    /// <summary>
    /// Utilities usable with dictionaries
    /// </summary>
    public static class DictionaryUtils
    {
        public class ExtensionPostfix<TKey, TValue> {
            readonly IDictionary<TKey, TValue> _dictionary;
            readonly TKey _key;

            internal ExtensionPostfix(IDictionary<TKey, TValue> dictionary, TKey key) {
                _dictionary = dictionary;
                _key = key;
            }


            /// <summary>
            /// Returns value from the dictionary by key, if it exists, otherwise returns default(<typeparamref name="TValue"/>).
            /// </summary>
            public TValue OrDefault() {
                TValue value;
                return _dictionary.TryGetValue(_key, out value) ? value : default(TValue);
            }


            /// <summary>
            /// Returns value from the dictionary by key, if it exists, otherwise returns specified <paramref name="defaultValue"/>.
            /// </summary>
            public TValue Or(TValue defaultValue) {
                TValue value;
                return _dictionary.TryGetValue(_key, out value) ? value : defaultValue;
            }


            /// <summary>
            /// Returns value from the dictionary by key, if it exists, otherwise returns value from '<paramref name="getDefaultValue"/>' callback.
            /// </summary>
            public TValue Or(Func<TValue> getDefaultValue) {
                TValue value;
                return _dictionary.TryGetValue(_key, out value) ? value : getDefaultValue();
            }
        }



        /// <summary>
        /// Returns value from <paramref name="dictionary"/> by <paramref name="key"/>, if it exists.
        /// Otherwise returns default(TValue).
        /// </summary>
        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
        
        /// <summary>
        /// Two-step extension for returning Value(by key).Or(something else).
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dictionary">An dictionary</param>
        /// <param name="key">An key</param>
        /// <returns>Second part of extension</returns>
        [MustUseReturnValue]
        public static ExtensionPostfix<TKey, TValue> Value<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            return new ExtensionPostfix<TKey, TValue>(dictionary, key);
        }
        
    }
}