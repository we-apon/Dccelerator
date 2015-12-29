using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection.Abstract;


namespace Dccelerator.Reflection
{
    public static class TypeInfo<TType>
    {
        #region Type property

        public static Type Type => _type ?? (_type = typeof (TType));

        public static System.Reflection.TypeInfo Info => _info ?? (_info = Type.GetTypeInfo());

        // ReSharper disable StaticMemberInGenericType
        static Type _type;
        static System.Reflection.TypeInfo _info;

        #endregion


        #region TryInvoke method

        static readonly Dictionary<string, Dictionary<Type, MethodDelegateBase>> _methods = new Dictionary<string, Dictionary<Type, MethodDelegateBase>>();

        // ReSharper restore StaticMemberInGenericType

        public static bool TryInvoke(object context, string methodName, params object[] args) {
            Dictionary<Type, MethodDelegateBase> methods;
            bool isGetted;
            lock (_methods) {
                isGetted = _methods.TryGetValue(methodName, out methods);
            }

            if (isGetted && methods == null)
                return false;


            if (methods == null) {
                var reflectionMethods = _type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.Name == methodName && !x.IsAbstract)
                    .ToArray();

                if (!reflectionMethods.Any()) {
                    lock (_methods) {
                        _methods[methodName] = null;
                    }
                    return false;
                }

                methods = new Dictionary<Type, MethodDelegateBase>();
                foreach (var methodInfo in reflectionMethods) {
                    var parameters = methodInfo.GetParameters();
                    var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();
                    var isAction = methodInfo.ReturnType == typeof (void);

                    var delegateType = isAction
                        ? TypeCache.ActionTypeWith(parameters.Length + 1).MakeGenericType(parameterTypes) //? parameters + context
                        : TypeCache.FuncTypeWith(parameters.Length + 2).MakeGenericType(parameterTypes); //? parameters + context + return


                    var methodDelegateType = typeof (DelegateContainer<>).MakeGenericType(delegateType);
                    var methodBase = Activator.CreateInstance(methodDelegateType) as MethodDelegateBase;
                    methods[methodDelegateType] = methodBase;
                }

                lock (_methods) {
                    _methods[methodName] = methods;
                }
            }

            throw new NotImplementedException();
        }

        #endregion



        #region Attributes

        // ReSharper disable StaticMemberInGenericType
        static readonly Dictionary<Type, object> _attributes = new Dictionary<Type, object>();
        static readonly Dictionary<Type, object> _inheritedAttributes = new Dictionary<Type, object>();
        static readonly Dictionary<Type, Attribute> _singleAttributes = new Dictionary<Type, Attribute>();
        // ReSharper restore StaticMemberInGenericType


        public static T[] GetAll<T>(bool inherit = true) where T : Attribute {
            object attributes;
            var attributeType = typeof (T);

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
            var attributeType = typeof (T);
            lock (_singleAttributes) {
                Attribute attribute;
                if (_singleAttributes.TryGetValue(attributeType, out attribute))
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
        static readonly Dictionary<Type, bool> _isAssignableFrom = new Dictionary<Type, bool>();


        public static bool IsAssignableFrom<T>() {
            return IsAssignableFrom(typeof (T));
        }


        public static bool IsAssignableFrom(Type type) {
            bool result;
            lock (_isAssignableFrom) {
                if (_isAssignableFrom.TryGetValue(type, out result))
                    return result;
            }

            result = Type.IsAssignableFrom(type);
            lock (_isAssignableFrom) {
                _isAssignableFrom[type] = result;
            }
            return result;
        }

        #endregion



        #region Properties

        // ReSharper disable StaticMemberInGenericType
        static readonly Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();
        static readonly Dictionary<string, PropertyPath> _propertyPaths = new Dictionary<string, PropertyPath>();
        static readonly Dictionary<Type, Dictionary<string, IProperty>> _neighborProperties = new Dictionary<Type, Dictionary<string, IProperty>>();

        // ReSharper restore StaticMemberInGenericType

        public static bool TryGetNestedProperty(object context, string path, out object value) {
            var propertyPath = get_me_property_path_for(path);
            if (propertyPath != null)
                return propertyPath.TryGetValueOfTargetProperty(context, out value);

            value = null;
            return false;
        }


        public static bool TrySetNestedProperty(object context, string path, object value) {
            var propertyPath = get_me_property_path_for(path);
            return propertyPath != null && propertyPath.TrySetTargetProperty(context, value);
        }


        /// <summary>
        /// Returns <see cref="IProperty"/> placed on <paramref name="path"/>
        /// </summary>
         public static IProperty Property( string path) {
            var propertyPath = get_me_property_path_for(path);
            return propertyPath?.GetTargetProperty();
        }


        static IProperty get_property(Type type, string name) {
            if (Type == type) {
                return get_property_from(type, _properties, name);
            }

            Dictionary<string, IProperty> neighborProps;
            lock (_neighborProperties) {
                _neighborProperties.TryGetValue(type, out neighborProps);
            }

            if (neighborProps == null) {
                var neighborInfo = typeof (TypeInfo<>).MakeGenericType(type);
                var neighborPropsField = neighborInfo.GetField("_properties", BindingFlags.Static | BindingFlags.NonPublic);
                if (neighborPropsField == null)
                    return null;

                neighborProps = (Dictionary<string, IProperty>) neighborPropsField.GetValue(null);
            }

            lock (_neighborProperties) {
                _neighborProperties[type] = neighborProps;
            }

            return get_property_from(type, neighborProps, name);
        }


        static IProperty get_property_from(Type type, Dictionary<string, IProperty> cache, string name) {
            IProperty property;
            lock (cache) {
                if (cache.TryGetValue(name, out property))
                    return property;
            }

            var prop = type.GetProperty(name); //todo: add BindingFlags
            if (prop == null)
                return null;

            var generic = typeof (Property<,>).MakeGenericType(type, prop.PropertyType);
            property = (IProperty) Activator.CreateInstance(generic, prop);


            lock (cache) {
                cache[name] = property;
            }
            return property;
        }


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
            return curPropPath.Property == null ? null : propertyPath;
        }


        static PropertyPath get_me_property_path_for( string path) {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            PropertyPath propertyPath;
            bool isGetted;
            lock (_propertyPaths)
                isGetted = _propertyPaths.TryGetValue(path, out propertyPath);

            if (isGetted)
                return propertyPath; //? can be null

            propertyPath = make_property_path(path);
            lock (_propertyPaths)
                _propertyPaths[path] = propertyPath;

            return propertyPath;
        }

        #endregion


    }
}