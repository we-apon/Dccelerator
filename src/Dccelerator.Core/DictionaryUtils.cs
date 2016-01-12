using System.Collections.Generic;


namespace Dccelerator
{
    /// <summary>
    /// Utilities usable with dictionaries
    /// </summary>
    public static class DictionaryUtils
    {
        /// <summary>
        /// Returns value from <paramref name="dictionary"/> by <paramref name="key"/>, if it exists.
        /// Otherwise returns default(TValue).
        /// </summary>
        public static TValue ValueOrDefault<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

        /// <summary>
        /// Returns value from <paramref name="dictionary"/> by <paramref name="key"/>, if it exists.
        /// Otherwise returns specified <paramref name="defaultValue"/>.
        /// </summary>
        public static TValue ValueOr<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TValue defaultValue, TKey key) {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        
        
    }
}