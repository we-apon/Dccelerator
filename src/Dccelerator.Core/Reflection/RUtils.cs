using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;


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
        /// Returns number of inheritance generator from <paramref name="parent"/> to <paramref name="child"/> type.
        /// </summary>
        /// <returns>
        /// Returns 0, if <paramref name="parent"/> is interface, and it can be assigranble from <paramref name="child"/>.
        /// Othrewise returns number of ingeritrance iterations fro <paramref name="parent"/> to <paramref name="child"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="parent"/> or <paramref name="child"/> is null</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="parent"/> and <paramref name="child"/> arguments are not siblings.</exception>
        public static int GetGenerationNumberTo(this Type parent, [NotNull] Type child) {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (child == null)
                throw new ArgumentNullException(nameof(child));

            if (!parent.IsAssignableFrom(child))
                throw new InvalidOperationException($"Type '{parent}' is not parent of type '{child}'");

            if (parent.GetInfo().IsInterface)
                return 0;

            var sum = 0;

            do {
                if (parent == child)
                    return sum;
                
                child = child.GetInfo().BaseType;
                sum++;
            } while (child != null);

            return sum;
        }

        
        public static PropertyInfo MostClosedPropertyNamed(string name, Type type) {
            return type.MostClosedPropertyNamed(name);
        }
        
        
        public static PropertyInfo MostClosedPropertyNamed(this Type type, string name) {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => x.Name == name)
                .MostClosedPropertyTo(type);
        }
        
        
        
        public static PropertyInfo MostClosedPropertyTo(Type type, IEnumerable<PropertyInfo> props) {
            return props.MostClosedPropertyTo(type);
        }
        
        
        
        public static PropertyInfo MostClosedPropertyTo(this IEnumerable<PropertyInfo> props, Type type) {
            Dictionary<Type, PropertyInfo> declarationMapping;
            try {
                declarationMapping = props.ToDictionary(x => x.DeclaringType, x => x);
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, e.ToString());
                throw;
            }

            if (!declarationMapping.Any())
                return null;
            
            var curType = type;
            do {
                if (declarationMapping.TryGetValue(curType, out PropertyInfo closestProperty))
                    return closestProperty;
            } while ((curType = curType.GetInfo().BaseType) != null);

            var error = $"Seems that passed properties doesn't belong to type '{type.FullName}'. "
                        + $"\nPassed props {{"
                        + $"\n\t{string.Join("\n,", declarationMapping.Select(x => $"\"{x.Key.FullName}\": \"{x.Value.Name}\""))}}}";
            Internal.TraceEvent(TraceEventType.Critical, error);
            throw new Exception(error);
        }
        
        
        
        
        

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