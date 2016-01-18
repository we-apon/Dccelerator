using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;


namespace Dccelerator.Reflection
{
    /// <summary>
    /// Reflection Utilities.
    /// Contains useful methods for manipulating types and it's members (mostly properties) in simple and fast manner.
    /// Methods of this class works using Expression Threes to generate and compile delegates to accessing properties much more faster than reflection.
    /// It also used caching of properties paths.
    /// </summary>
    /// <remarks>
    /// Currently it invokes every property through delegate, to access nested properties.
    /// In future, I will generate and use delegate accessing full requested property path at one call.
    /// </remarks>
    /// <seealso cref="RUtils{TType}"/>
    public static class RUtils //todo: generate and use delegate accessing full requested property path at one call.
    {

        /// <summary>
        /// Returns an structure for manipulating property what placed on <paramref name="path"/>, if we start looking from specified <paramref name="contextType"/>.
        /// Is will return null, when path is wrong.
        /// </summary>
        /// <param name="contextType"></param> //todo: documentation
        /// <param name="path"></param>
        /// <returns></returns>
        public static PropertyPath GetPropertyPath(this Type contextType, string path) {

            var identity = new PropIdentity(contextType, path);

            PropertyPath propertyPath;
            if (_propertyPaths.TryGetValue(identity, out propertyPath))
                return propertyPath;

            propertyPath = make_property_path(identity);
            if (!_propertyPaths.TryAdd(identity, propertyPath))
                propertyPath = _propertyPaths[identity];

            return propertyPath;
        }


        /// <summary>
        /// Returns value of static property placed on <paramref name="path"/> with started point at <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type that has static property on passed <paramref name="path"/></param>
        /// <param name="path">Path to requested property</param>
        /// <param name="value">Value</param>
        public static bool TryGetValueOnPath(this Type type, string path, out object value) {
            if (type == null || string.IsNullOrWhiteSpace(path)) {
                value = null;
                return false;
            }

            return try_get_value_on_path(type, null, path, out value);
        }



        /// <summary>
        /// Returns value of property placed on <paramref name="path"/> with started point at <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Object that has property on passed <paramref name="path"/></param>
        /// <param name="path">Path to requested property</param>
        /// <param name="value">Value</param>
        public static bool TryGetValueOnPath(this object context, string path, out object value) {
            if (context == null || string.IsNullOrWhiteSpace(path)) {
                value = null;
                return false;
            }

            return try_get_value_on_path(context.GetType(), context, path, out value);
        }

        

        public static bool TrySetValueOnPath(this object context, string path, object value) {

            PropertyPath propertyPath;

            return context != null 
                && !string.IsNullOrWhiteSpace(path) 
                && (propertyPath = GetPropertyPath(context.GetType(), path)) != null
                && propertyPath.TrySetTargetProperty(context, value);
        }
        



        #region private

        static bool try_get_value_on_path(Type type, object context, string path, out object value) {
            var propertyPath = GetPropertyPath(type, path);
            if (propertyPath == null) {
                value = null;
                return false;
            }

            return propertyPath.TryGetValueOfTargetProperty(context, out value);
        }


        class PropIdentity
        {
            public readonly string Path;

            public readonly Type Type;

            readonly int _hashCode;


            public PropIdentity(Type type, string path) {
                Type = type;
                Path = path;

                var h = 13;
                h *= 7 + Path.GetHashCode();
                h *= 7 + Type.GetHashCode();
                _hashCode = h;
            }


            public override int GetHashCode() {
                return _hashCode;
            }


            public override bool Equals(object obj) {
                return Equals(obj as PropIdentity);
            }


            public bool Equals(PropIdentity other) {
                return other != null && other.Path == Path && other.Type == Type;
            }
        }


        static readonly ConcurrentDictionary<PropIdentity, PropertyPath> _propertyPaths = new ConcurrentDictionary<PropIdentity, PropertyPath>();


        static PropertyPath make_property_path( PropIdentity identity) {
            var neighborInfo = typeof (RUtils<>).MakeGenericType(identity.Type);
            var neighborMethod = neighborInfo.GetMethod("make_property_path", BindingFlags.Static | BindingFlags.NonPublic);
            return (PropertyPath) neighborMethod?.Invoke(null, new object[] {identity.Path});
        }

        #endregion


    }
}