using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;


namespace Dccelerator.Reflection
{
    public static class TypeCache
    {
        public static readonly Type Void = typeof (void);
        public static readonly Type ObjectType = typeof (object);
        public static readonly Type StringType = typeof (string);
        public static readonly Type GuidType = typeof (Guid);
        public static readonly Type EnumerableType = typeof (IEnumerable);
        public static readonly Type ListType = typeof (IList);
        public static readonly Type ConvertibleType = typeof (IConvertible);
        /*        public static readonly Type XmlElementAttributeType = typeof (XmlElementAttribute);
                public static readonly Type XmlTypeAttributeType = typeof (XmlTypeAttribute);
                public static readonly Type DataMemberAttributeType = typeof (DataMemberAttribute);
                public static readonly Type DataContractAttributeType = typeof (DataContractAttribute);*/

#if !NET40
        static readonly ConcurrentDictionary<Type, TypeInfo> _typeInfos = new ConcurrentDictionary<Type, TypeInfo>();
#endif

        static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _typeProperties = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

        static readonly ConcurrentDictionary<Type, HashSet<Type>> _typeParants = new ConcurrentDictionary<Type, HashSet<Type>>();
        static readonly ConcurrentDictionary<Type, HashSet<Type>> _typeInterfaces = new ConcurrentDictionary<Type, HashSet<Type>>();
        static readonly ConcurrentDictionary<Type, bool> _isStrongEnumerable = new ConcurrentDictionary<Type, bool>();
        static readonly ConcurrentDictionary<Type, object> _defaultInstances = new ConcurrentDictionary<Type, object>();
        static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>> _isAssiblableFrom = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>>();
        static readonly ConcurrentDictionary<Type, Type> _collectionElementTypes = new ConcurrentDictionary<Type, Type>();
        static readonly ConcurrentDictionary<Type, Type[]> _genericArguments = new ConcurrentDictionary<Type, Type[]>();


        static readonly Type[] _funcTypes = {
            typeof (Func<>),
            typeof (Func<,>),
            typeof (Func<,,>),
            typeof (Func<,,,>),
            typeof (Func<,,,,>),
            typeof (Func<,,,,,>),
            typeof (Func<,,,,,,>),
            typeof (Func<,,,,,,,>),
            typeof (Func<,,,,,,,,>),
            typeof (Func<,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,>),
            typeof (Func<,,,,,,,,,,,,,,,,>)
        };

        static readonly Type[] _actionTypes = {
            typeof (Action<>),
            typeof (Action<,>),
            typeof (Action<,,>),
            typeof (Action<,,,>),
            typeof (Action<,,,,>),
            typeof (Action<,,,,,>),
            typeof (Action<,,,,,,>),
            typeof (Action<,,,,,,,>),
            typeof (Action<,,,,,,,,>),
            typeof (Action<,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,>),
            typeof (Action<,,,,,,,,,,,,,,,>),
        };


        public static Type FuncTypeWith(int argumentsCount) {
            return _funcTypes[argumentsCount];
        }


        public static Type ActionTypeWith(int argumentsCount) {
            return _actionTypes[argumentsCount];
        }


        public static Type[] GenericsOf(Type type) {
            Type[] generics;

            if (_genericArguments.TryGetValue(type, out generics))
                return generics;

#if NET40
            generics = type.GetGenericArguments();
#else
            generics = type.GenericTypeArguments;
#endif

            _genericArguments[type] = generics;

            return generics;
        }


        public static Type ElementTypeOf(Type collectionType) {
            Type elementType;

            if (_collectionElementTypes.TryGetValue(collectionType, out elementType))
                return elementType;

            if (collectionType.IsArray)
                elementType = collectionType.GetElementType();
            else {
                var generics = GenericsOf(collectionType);
                elementType = generics.Length > 0 ? generics[0] : ObjectType;
            }

            _collectionElementTypes[collectionType] = elementType;
            return elementType;
        }


        public static bool IsAssignableFrom(Type targetType, Type valueType) {
            bool result;

            ConcurrentDictionary<Type, bool> assignables;
            if (!_isAssiblableFrom.TryGetValue(targetType, out assignables)) {
                assignables = new ConcurrentDictionary<Type, bool>();
                _isAssiblableFrom[targetType] = assignables;
            }

            if (assignables.TryGetValue(valueType, out result))
                return result;


            result = targetType.IsAssignableFrom(valueType);
            assignables[valueType] = result;


            return result;
        }


        public static object DefaultInstanceOf(Type type) {
            if (GetInfo(type).IsClass)
                return null;

            object instance;


            if (_defaultInstances.TryGetValue(type, out instance))
                return instance;

            try {
                instance = Activator.CreateInstance(type);
            }
            catch (Exception) {
                instance = null;
            }


            _defaultInstances[type] = instance;

            return instance;
        }



        public static bool IsInherited(this Type childType, Type parentType) {
            return ParentsOf(childType).Contains(parentType) || InterfacesOf(childType).Contains(parentType);
        }



        /// <summary>
        /// Returns <see langword="true"/>, if <paramref name="type"/> is an collection (e.g. an <seealso cref="IEnumerable"/> and not <see cref="String"/>).
        /// </summary>
        public static bool IsAnCollection(this Type type) {
            bool result;

            if (_isStrongEnumerable.TryGetValue(type, out result))
                return result;

            result = StringType != type && !InterfacesOf(type).Contains(EnumerableType);
            _isStrongEnumerable[type] = result;

            return result;
        }




        public static HashSet<Type> InterfacesOf(this Type type) {
            HashSet<Type> interfaces;


            if (_typeInterfaces.TryGetValue(type, out interfaces))
                return interfaces;

            interfaces = new HashSet<Type>(type.GetInterfaces());
            _typeInterfaces[type] = interfaces;


            return interfaces;
        }



        public static HashSet<Type> ParentsOf(this Type type) {
            HashSet<Type> parents;


            if (_typeParants.TryGetValue(type, out parents))
                return parents;

            parents = new HashSet<Type>();
            FillParentsOf(type, ref parents);

            _typeParants[type] = parents;


            return parents;
        }


        static void FillParentsOf(Type type, ref HashSet<Type> parents) {
            var baseType = GetInfo(type).BaseType;
            if (baseType == null)
                return;

            parents.Add(baseType);
            FillParentsOf(baseType, ref parents);
        }




#if NET40

        /// <summary>
        /// Return <see cref="Type"/> instance that was passed as <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// This extension method exists just for .net 4.0 api compatibility with modern target platforms
        /// </remarks>
        public static Type GetInfo(this Type type) {
            return type;
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this Type type) {
            return type.GetCustomAttributes(true).Cast<Attribute>();
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type type) where TAttribute : Attribute {
            return type.GetCustomAttributes(true).OfType<TAttribute>();
        }


#else
        public static TypeInfo GetInfo(this Type type) {
            TypeInfo info;

            if (_typeInfos.TryGetValue(type, out info))
                return info;


            info = type.GetTypeInfo();
            return !_typeInfos.TryAdd(type, info) 
                ? _typeInfos[type] 
                : info;
        }
#endif




        public static Dictionary<string, PropertyInfo> Properties(this Type type) {
            Dictionary<string, PropertyInfo> properties;
            if (_typeProperties.TryGetValue(type, out properties))
                return properties;


            properties = new Dictionary<string, PropertyInfo>();
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static)) {
                PropertyInfo storedProperty;
                if (!properties.TryGetValue(prop.Name, out storedProperty)) {
                    properties.Add(prop.Name, prop);
                    continue;
                }

                properties[prop.Name] = (PropertyInfo) TopInHierarchy(type, prop, storedProperty);
            }

            return _typeProperties.TryAdd(type, properties)
                ? properties
                : _typeProperties[type];
        }




        /// <summary>
        /// Returns <see cref="MemberInfo"/>, declared on upper in <paramref name="type"/> hierarchy.
        /// This's usefull to select <see cref="MemberInfo"/> declared with <see langword="new"/> keyword and hiding another.
        /// </summary>
        public static MemberInfo TopInHierarchy(Type type, MemberInfo one, MemberInfo two) {
            if (one.DeclaringType == type)
                return one;

            if (two.DeclaringType == type)
                return two;

            return TopInHierarchy(GetInfo(type).BaseType, one, two);
        }







    }
}