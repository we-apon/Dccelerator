using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// Returns number of inheritance generator from <paramref name="parent"/> to <paramref name="child"/> type.
        /// </summary>
        /// <returns>
        /// Returns 0, if <paramref name="parent"/> is interface, and it can be assigranble from <paramref name="child"/>.
        /// Othrewise returns number of ingeritrance iterations fro <paramref name="parent"/> to <paramref name="child"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="parent"/> or <paramref name="child"/> is null</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="parent"/> and <paramref name="child"/> arguments are not siblings.</exception>
        public static int GetGenerationNumberTo(this Type parent, Type child) {
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
                Log.TraceEvent(TraceEventType.Error, e.ToString());
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
            Log.TraceEvent(TraceEventType.Critical, error);
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

            if (_propertyPaths.TryGetValue(identity, out var propertyPath))
                return propertyPath;

            propertyPath = make_property_path(identity);
            if (!_propertyPaths.TryAdd(identity, propertyPath))
                propertyPath = _propertyPaths[identity];
                
            return propertyPath;
        }

        

        
        /// <summary>
        /// Returns value of property placed on <paramref name="path"/> with started point at <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Object that has property on passed <paramref name="path"/></param>
        /// <param name="path">Path to requested property</param>
        /// <param name="value">Value</param>
        public static bool TryGet<T>(this T context, string path, out object value) {
            return RUtils<T>.TryGet(context, path, out value);
        }

        
        /// <summary>
        /// Returns value of property placed on <paramref name="path"/> with started point at <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Object that has property on passed <paramref name="path"/></param>
        /// <param name="path">Path to requested property</param>
        /// <param name="value">Value</param>
        public static bool TryGet(this object context, string path, out object value) {
            if (context == null || path == null) {
                value = null;
                return false;
            }

            var type = context.GetType();
            var prop = GetPropertyPath(type, path);
            if (prop != null) {
                try {
                    value = prop.Get(context);
                    return true;
                }
                catch (Exception e) {
                    Log.TraceEvent(TraceEventType.Error, $"Can't get value from path {type.FullName}.{path}\n\n{e}");
                    value = null;
                    return false;
                }
            }

            Log.TraceEvent(TraceEventType.Warning, $"There is no property or field {type.FullName}.{path}");
            value = null;
            return false;
        }



        public static object Get<T>(this T context, string path) {
            return RUtils<T>.Get(context, path);
        }

        public static object Get(this object context, string path) {
            if (context == null || path == null)
                return null;
            
            var type = context.GetType();
            var propertyPath = GetPropertyPath(type, path);
            if (propertyPath != null)
                return propertyPath.Get(context);
            
            Log.TraceEvent(TraceEventType.Warning, $"There is no property or field {type.FullName}.{path}");
            return null;
        }



        public static bool TrySet<T>(this T context, string path, object value) {
            if (context == null || string.IsNullOrWhiteSpace(path))
                return false;

            return RUtils<T>.TrySet(context, path, value);
        }


        public static bool TrySet(this object context, string path, object value) {
            if (context == null || path == null)
                return false;

            var type = context.GetType();

            var prop = GetPropertyPath(type, path);
            if (prop != null) {
                try {
                    prop.Set(context, value);
                    return true;
                }
                catch (Exception e) {
                    Log.TraceEvent(TraceEventType.Error, $"Can't set value '{value}' on path {type.FullName}.{path}\n\n{e}");
                    return false;
                }
            }

            Log.TraceEvent(TraceEventType.Warning, $"There is no property or field {type.FullName}.{path}");
            return false;
        }


        public static void Set<T>(this T context, string path, object value) {
            RUtils<T>.Set(context, path, value);
        }

        public static void Set(this object context, string path, object value) {
            if (context == null || path == null)
                return;

            PropertyPath propertyPath;
            if ((propertyPath = GetPropertyPath(context.GetType(), path)) != null) {
                propertyPath.Set(context, value);
            }
        }




        #region private


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