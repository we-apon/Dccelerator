using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;


namespace Dccelerator.Reflection
{
    public class TypeCache
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


        
        static readonly ConcurrentDictionary<Type, System.Reflection.TypeInfo> _typeInfos = new ConcurrentDictionary<Type, System.Reflection.TypeInfo>(); 
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

            generics = type.GenericTypeArguments;


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
            if (InfoOf(type).IsClass)
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



        public static bool IsInherited(Type childType, Type parentType) {
            return ParentsOf(childType).Contains(parentType) || InterfacesOf(childType).Contains(parentType);
        }



        public static bool IsStrongEnumerable(Type type) {
            bool result;

            if (_isStrongEnumerable.TryGetValue(type, out result))
                return result;

            result = StringType != type && !InterfacesOf(type).Contains(EnumerableType);
            _isStrongEnumerable[type] = result;

            return result;
        }




        public static HashSet<Type> InterfacesOf(Type type) {
            HashSet<Type> interfaces;


            if (_typeInterfaces.TryGetValue(type, out interfaces))
                return interfaces;

            interfaces = new HashSet<Type>(type.GetInterfaces());
            _typeInterfaces[type] = interfaces;


            return interfaces;
        }



        public static HashSet<Type> ParentsOf(Type type) {
            HashSet<Type> parents;


            if (_typeParants.TryGetValue(type, out parents))
                return parents;

            parents = new HashSet<Type>();
            FillParentsOf(type, ref parents);

            _typeParants[type] = parents;


            return parents;
        }


        static void FillParentsOf(Type type, ref HashSet<Type> parents) {
            var baseType = InfoOf(type).BaseType;
            if (baseType == null)
                return;

            parents.Add(baseType);
            FillParentsOf(baseType, ref parents);
        }





        public static System.Reflection.TypeInfo InfoOf(Type type) {
            System.Reflection.TypeInfo info;

            if (_typeInfos.TryGetValue(type, out info))
                return info;


            info = type.GetTypeInfo();
            return !_typeInfos.TryAdd(type, info) 
                ? _typeInfos[type] 
                : info;
        }


    }
}