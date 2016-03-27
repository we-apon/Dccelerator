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
        public static readonly Type DateTimeType = typeof (DateTime);
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
        /*static readonly ConcurrentDictionary<Type, object> _defaultInstances = new ConcurrentDictionary<Type, object>();*/
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


        /// <summary>
        /// Returns type of element of <paramref name="collectionType"/>.
        /// </summary>
        /// <returns>Returns null, if <paramref name="collectionType"/> is not an collection at all.</returns>
        public static Type ElementType(this Type collectionType) {
            Type elementType;

            if (_collectionElementTypes.TryGetValue(collectionType, out elementType))
                return elementType;

            if (collectionType.IsArray) //todo: check caching performance
                elementType = collectionType.GetElementType();
            else if (EnumerableType.IsAssignableFrom(collectionType)) {
                var generics = GenericsOf(collectionType);
                elementType = generics.Length > 0 ? generics[0] : ObjectType;
            }

            return _collectionElementTypes.TryAdd(collectionType, elementType) 
                ? elementType 
                : _collectionElementTypes[collectionType];
        }


        public static bool IsAssignableFrom(Type targetType, Type valueType) {
            bool result;

            ConcurrentDictionary<Type, bool> assignables;
            if (!_isAssiblableFrom.TryGetValue(targetType, out assignables)) {
                assignables = new ConcurrentDictionary<Type, bool>();
                if (!_isAssiblableFrom.TryAdd(targetType, assignables))
                    assignables = _isAssiblableFrom[targetType];
            }

            if (assignables.TryGetValue(valueType, out result))
                return result;


            result = targetType.IsAssignableFrom(valueType);
            
            return assignables.TryAdd(valueType, result)
                ? result
                : assignables[valueType];
        }

/*

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


            return _defaultInstances.TryAdd(type, instance)
                ? instance
                : _defaultInstances[type];
        }
*/



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

            result = type == EnumerableType || (type != StringType && InterfacesOf(type).Contains(EnumerableType));
            _isStrongEnumerable[type] = result;

            return result;
        }




        public static HashSet<Type> InterfacesOf(this Type type) {
            HashSet<Type> interfaces;


            if (_typeInterfaces.TryGetValue(type, out interfaces))
                return interfaces;

            interfaces = new HashSet<Type>(type.GetInterfaces());

            return _typeInterfaces.TryAdd(type, interfaces)
                ? interfaces
                : _typeInterfaces[type];
        }



        public static HashSet<Type> ParentsOf(this Type type) {
            HashSet<Type> parents;


            if (_typeParants.TryGetValue(type, out parents))
                return parents;

            parents = new HashSet<Type>();
            FillParentsOf(type, ref parents);

            return _typeParants.TryAdd(type, parents)
                ? parents
                : _typeParants[type];
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
        /// <summary>
        /// Returns globally cached info about <paramref name="type"/>.
        /// </summary>
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





        /// <summary>
        /// Returns properties getted with <paramref name="bindingFlags"/> and filtered with specified <paramref name="filter"/>.
        /// Properties are distinct by name with <see cref="TopInHierarchy{TMemberInfo}"/> method.
        /// </summary>
        /// <param name="type">An type</param>
        /// <param name="bindingFlags">Used binding flags</param>
        /// <param name="filter">Additional filter for properties. If not specified - allowed all properties.</param>
        /// <seealso cref="Properties(Type)"/>
        /// <seealso cref="TopInHierarchy{TMemberInfo}"/>
        public static Dictionary<string, PropertyInfo> Properties(this Type type, BindingFlags bindingFlags, Func<PropertyInfo, bool> filter = null) {
            filter = filter ?? _allowAllProperties;

            var properties = new Dictionary<string, PropertyInfo>();
            foreach (var prop in type.GetProperties(bindingFlags)) {
                if (!filter(prop))
                    continue;

                PropertyInfo storedProperty;
                if (!properties.TryGetValue(prop.Name, out storedProperty)) {
                    properties.Add(prop.Name, prop);
                    continue;
                }

                properties[prop.Name] = TopInHierarchy(type, prop, storedProperty);
            }
            return properties;
        }


        static readonly Func<PropertyInfo, bool> _allowAllProperties = prop => true;


        /// <summary>
        /// Returns all properties of the <paramref name="type"/>.
        /// Properties resolves using binding flags: (Public | NonPublic | FlattenHierarchy | Instance | Static).
        /// Properties are distinct by name with <see cref="TopInHierarchy{TMemberInfo}"/> method.
        /// </summary>
        /// <seealso cref="Properties(Type, BindingFlags, Func{PropertyInfo,bool})"/>
        /// <seealso cref="TopInHierarchy{TMemberInfo}"/>
        public static Dictionary<string, PropertyInfo> Properties(this Type type) {
            Dictionary<string, PropertyInfo> properties;
            if (_typeProperties.TryGetValue(type, out properties))
                return properties;

            properties = Properties(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static);

            return _typeProperties.TryAdd(type, properties)
                ? properties
                : _typeProperties[type];
        }




        /// <summary>
        /// Returns <see cref="MemberInfo"/>, declared on upper in <paramref name="type"/> hierarchy.
        /// This is usefull to select <see cref="MemberInfo"/> declared with <see langword="new"/> keyword and hiding another.
        /// </summary>
        public static TMemberInfo TopInHierarchy<TMemberInfo>(Type type, TMemberInfo one, TMemberInfo two) where  TMemberInfo : MemberInfo {
            if (one.DeclaringType == type)
                return one;

            if (two.DeclaringType == type)
                return two;

            return TopInHierarchy(GetInfo(type).BaseType, one, two);
        }




        /// <summary>
        /// <see cref="Activator"/>.<see cref="Activator.CreateInstance(Type)"/> as extension.
        /// </summary>
        public static object CreateInstance(this Type type) {
            return Activator.CreateInstance(type);
        }


        /// <summary>
        /// <see cref="Activator"/>.<see cref="Activator.CreateInstance(Type, object[])"/> as extension.
        /// </summary>
        public static object CreateInstance(this Type type, params object[] args) {
            return Activator.CreateInstance(type, args);
        }




    }
}