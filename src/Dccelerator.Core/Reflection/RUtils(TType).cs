using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FastMember;
using JetBrains.Annotations;


namespace Dccelerator.Reflection {

    
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
    /// <seealso cref="RUtils"/>
    public static class RUtils<TType> {

        public static readonly Type Type = typeof(TType);

#if NET40
        public static readonly Type Info = Type;
#else
        public static readonly TypeInfo Info = Type.GetTypeInfo();
#endif

        static readonly TypeAccessor _accessor = TypeAccessor.Create(Type);


        #region Attributes

        // ReSharper disable StaticMemberInGenericType
        static readonly Dictionary<Type, object> _attributes = new Dictionary<Type, object>();
        static readonly Dictionary<Type, object> _inheritedAttributes = new Dictionary<Type, object>();

        static readonly Dictionary<Type, Attribute> _singleAttributes = new Dictionary<Type, Attribute>();

        // ReSharper restore StaticMemberInGenericType







        public static T[] GetAll<T>(bool inherit = true) where T : Attribute {
            object attributes;
            var attributeType = typeof(T);

            if (inherit) {
                lock (_inheritedAttributes) {
                    if (_inheritedAttributes.TryGetValue(attributeType, out attributes))
                        return (T[]) attributes;
                }
            }
            else {
                lock (_attributes) {
                    if (_attributes.TryGetValue(attributeType, out attributes))
                        return (T[]) attributes;
                }
            }

            var attribs = Info.GetCustomAttributes(attributeType, inherit).Cast<T>().ToArray();


            if (inherit) {
                lock (_inheritedAttributes) {
                    _inheritedAttributes[attributeType] = attribs;
                }
            }
            else {
                lock (_attributes) {
                    _attributes[attributeType] = attribs;
                }
            }

            return attribs;
        }


        public static T Get<T>(bool inherit = true) where T : Attribute {
            var attributeType = typeof(T);
            lock (_singleAttributes) {
                if (_singleAttributes.TryGetValue(attributeType, out var attribute))
                    return (T) attribute;
            }


            var attrib = GetAll<T>(inherit).SingleOrDefault();
            lock (_singleAttributes) {
                _singleAttributes[attributeType] = attrib;
            }

            return attrib;
        }

        #endregion



        #region IsAssignableFrom<T>

        // ReSharper disable once StaticMemberInGenericType
        static readonly ConcurrentDictionary<Type, bool> _isAssignableFrom = new ConcurrentDictionary<Type, bool>();


        public static bool IsAssignableFrom<T>() {
            return IsAssignableFrom(typeof(T));
        }


        public static bool IsAssignableFrom(Type type) {
            if (_isAssignableFrom.TryGetValue(type, out var result))
                return result;

            result = Type.IsAssignableFrom(type);
            _isAssignableFrom.TryAdd(type, result);
            return result;
        }

        #endregion



        #region Properties




        // ReSharper disable StaticMemberInGenericType
        static readonly ConcurrentDictionary<string, IProperty> _properties = new ConcurrentDictionary<string, IProperty>();

        static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, IProperty>> _neighborProperties = new ConcurrentDictionary<Type, ConcurrentDictionary<string, IProperty>>();
        // ReSharper restore StaticMemberInGenericType


        /// <exception cref="InvalidOperationException">There is no such property or field.</exception>
        public static object Get(TType context, string path) {
            if (_pathsByCode.TryGetValue(path.GetHashCode(), out var propPath) || _paths.TryGetValue(path, out propPath))
                return propPath.Get(context);

            throw new InvalidOperationException($"There is no property or field {Type.FullName}.{path}");
        }


        /// <summary>
        /// Returns value of property placed on <paramref name="path"/> with started point at <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        /// Object that has property on passed <paramref name="path"/>. 
        /// It can be <see langword="null"/> for static properties.
        /// </param>
        /// <param name="path">Path to requested property</param>
        /// <param name="value">Value</param>
        public static bool TryGet(TType context, string path, out object value) {
            if (_pathsByCode.TryGetValue(path.GetHashCode(), out var propPath) || _paths.TryGetValue(path, out propPath)) {
                try {
                    value = propPath.Get(context);
                    return true;
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Error, $"Can't get value from path {Type.FullName}.{path}\n\n{e}");
                    value = null;
                    return false;
                }
            }

            Internal.TraceEvent(TraceEventType.Warning, $"There is no property or field {Type.FullName}.{path}");

            value = null;
            return false;
        }


        /*
        public static object Get(TType context, string name) {
            return _accessor[context, name];
        }


        public static bool TryGet(object context, string path, out object value) {
            try {
                value = _accessor[context, path];
                return true;
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, $"Can't get value {Type.FullName}.{path}\n\n{e}");
                value = null;
                return false;
            }
        }*/



        /// <exception cref="InvalidOperationException">There is no such property or field.</exception>
        public static void Set(TType context, string path, object value) {
            if (_pathsByCode.TryGetValue(path.GetHashCode(), out var propPath) || _paths.TryGetValue(path, out propPath)) {
                propPath.Set(context, value);
                return;
            }

            throw new InvalidOperationException($"There is no property or field {Type.FullName}.{path}");
        }


        public static bool TrySet(TType context, string path, object value) {
            if (_pathsByCode.TryGetValue(path.GetHashCode(), out var propPath) || _paths.TryGetValue(path, out propPath)) {
                try {
                    propPath.Set(context, value);
                    return true;
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Error, $"Can't set value '{value}' on path {Type.FullName}.{path}\n\n{e}");
                    return false;
                }
            }

            Internal.TraceEvent(TraceEventType.Warning, $"There is no property or field {Type.FullName}.{path}");
            return false;
        }


        /*
        public static void Set(TType context, string name, object value) {
            _accessor[context, name] = value;
        }

        public static bool TrySet(object context, string name, object value) {
            try {
                _accessor[context, name] = value;
                return true;
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, $"Can't set value '{value}' on path {Type.FullName}.{name}\n\n{e}");
                return false;
            }
        }*/


/*
        public static PropertyPath GetPath(string path) {
            if (_pathsByCode.TryGetValue(path.GetHashCode(), out var propPath) || _paths.TryGetValue(path, out propPath))
                return propPath;

            return null;
        }


        public static bool TryGetPath(string path, out PropertyPath propertyPath) {
            return _pathsByCode.TryGetValue(path.GetHashCode(), out propertyPath) || _paths.TryGetValue(path, out propertyPath);
        }


        /// <summary>
        /// Returns <see cref="IProperty"/> placed on <paramref name="path"/>
        /// </summary>
        public static IProperty GetProperty(string path) {
            if (_pathsByCode.TryGetValue(path.GetHashCode(), out var propertyPath) || _paths.TryGetValue(path, out propertyPath)) {
                return propertyPath.GetTargetProperty();
            }

            return null;
        }
        */

        


        static IProperty get_property(Type type, string name) {
            if (Type == type) {
                return get_property_from(type, _properties, name);
            }

            if (_neighborProperties.TryGetValue(type, out var neighborProps))
                return get_property_from(type, neighborProps, name);

            var neighborInfo = typeof(RUtils<>).MakeGenericType(type);
            var neighborPropsField = neighborInfo.GetField("_properties", BindingFlags.Static | BindingFlags.NonPublic);
            if (neighborPropsField == null)
                return null;

            neighborProps = (ConcurrentDictionary<string, IProperty>) neighborPropsField.GetValue(null);
            _neighborProperties.TryAdd(type, neighborProps);
            return get_property_from(type, neighborProps, name);
        }


        public static PropertyInfo MostClosedPropertyNamed(string name) {
            return Type.MostClosedPropertyNamed(name);
        }


        static IProperty get_property_from(Type type, ConcurrentDictionary<string, IProperty> cache, string name) {
            if (cache.TryGetValue(name, out var property))
                return property;

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => x.Name == name)
                .ToList();

            if (props.Count > 1)
                Internal.TraceEvent(TraceEventType.Warning, $"Type {type.FullName} contains {props.Count} properties named {name}. Most closely declared property will be selected for reflection access through Dccelerator API, so make sure that here will not be any collisions");

            var prop = props.MostClosedPropertyTo(type);
            if (prop == null)
                return null;

            var generic = typeof(Property<,>).MakeGenericType(type, prop.PropertyType);
            property = (IProperty) Activator.CreateInstance(generic, prop);

            cache.TryAdd(name, property);
            return property;
        }


        [CanBeNull]
        static PropertyPath make_property_path(string path) {
            var propertyPath = new PropertyPath();

            var nextPath = path;
            var dotIdx = path.IndexOf('.');
            var curPropPath = propertyPath;
            var type = Type;

            while (dotIdx > -1) {
                var name = nextPath.Substring(0, dotIdx);
                nextPath = nextPath.Substring(dotIdx + 1);

                curPropPath.Property = get_property(type, name);
                if (curPropPath.Property == null)
                    return null;

                type = curPropPath.Property.Info.PropertyType;
                curPropPath.Nested = new PropertyPath();
                curPropPath = curPropPath.Nested;

                dotIdx = nextPath.IndexOf('.');
            }

            curPropPath.Property = get_property(type, nextPath);
            if (curPropPath == null)
                return null;

            propertyPath.Build();
            return propertyPath;
        }
        

        #endregion






        static readonly Dictionary<string, IProperty> _props = new Dictionary<string, IProperty>();
        static readonly Dictionary<int, IProperty> _byCode = new Dictionary<int, IProperty>();

        static readonly Dictionary<string, PropertyPath> _paths = new Dictionary<string, PropertyPath>();
        static readonly Dictionary<int, PropertyPath> _pathsByCode = new Dictionary<int, PropertyPath>();


        static RUtils() {
            foreach (var path in GetPaths()) {
                PreparePath(path);
            }

            var notUniqueCodes = new HashSet<int>();

            foreach (var prop in Type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                var generic = typeof(Property<,>).MakeGenericType(typeof(TType), prop.PropertyType);
                var property = (IProperty)Activator.CreateInstance(generic, prop);

                var code = prop.Name.GetHashCode();
                if (_byCode.ContainsKey(code)) {
                    notUniqueCodes.Add(code);
                    _byCode.Remove(code);
                    _props.Add(prop.Name, property);
                } else if (notUniqueCodes.Contains(code)) {
                    _props.Add(prop.Name, property);
                }
                else {
                    _byCode.Add(code, property);
                }
            }
        }


        static void PreparePath(PropPath path, HashSet<int> notUniqueCodes = null, string prefix = null) {

            notUniqueCodes = notUniqueCodes ?? new HashSet<int>();

            prefix = $"{prefix}.{path.Info.Name}".TrimStart('.');
            
            var propPath = make_property_path(prefix);


            var code = prefix.GetHashCode();
            if (_pathsByCode.ContainsKey(code)) {
                notUniqueCodes.Add(code);
                _pathsByCode.Remove(code);
                _paths.Add(prefix, propPath);
            }
            else if (notUniqueCodes.Contains(code)) {
                _paths.Add(prefix, propPath);
            }
            else {
                _pathsByCode.Add(code, propPath);
            }

            foreach (var child in path.Children) {
                PreparePath(child, notUniqueCodes, prefix);
            }
        }





        class PropPath {
            public int Dept { get; set; }
            public PropertyInfo Info { get; set; }
            public PropPath Owner { get; set; }
            public PropPath[] Children { get; set; }
        }


        static PropPath[] GetPaths() {
            var paths = Type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Select(x => {
                    var path = new PropPath {
                        Dept = 1,
                        Info = x,
                        Owner = null
                    };
                    path.Children = GetPaths(path);
                    return path;
                }).ToArray();
            return paths;
        }



        static PropPath[] GetPaths(PropPath owner) {

            // 'Types' recursion is not checked for suppoting deep paths of hierarchical entities, like 'Parent.Parent.Parent...'

            if (owner.Dept >= 10) // think 10 is quite deep
                return new PropPath[0];

            var type = owner.Info.PropertyType;
            if (typeof(IEnumerable).IsAssignableFrom(type) || type.GetInfo().IsPrimitive)
                return new PropPath[0];

            var paths = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Select(x => GetPath(x, owner)) //? recursion
                .ToArray();

            return paths;
        }


        static PropPath GetPath(PropertyInfo info, PropPath owner) {
            var path = new PropPath {
                Dept = owner.Dept + 1,
                Info = info,
                Owner = owner,
            };
            path.Children = GetPaths(path); //? recursion to GetPaths method
            return path;
        }


    }
}