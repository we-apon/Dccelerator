using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;


namespace Dccelerator.Reflection
{
    public static class TypeManipulator
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




        public static bool TryGetValueOnPath(this object context, string path, out object value) {
            PropertyPath propertyPath;

            if (context == null || string.IsNullOrWhiteSpace(path) || (propertyPath = GetPropertyPath(context.GetType(), path)) == null) {
                value = null;
                return false;
            }

            return propertyPath.TryGetValueOfTargetProperty(context, out value);
        }

        

        public static bool TrySetValueOnPath(this object context, string path, object value) {

            PropertyPath propertyPath;

            return context != null 
                && !string.IsNullOrWhiteSpace(path) 
                && (propertyPath = GetPropertyPath(context.GetType(), path)) != null
                && propertyPath.TrySetTargetProperty(context, value);
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
            var neighborInfo = typeof (TypeManipulator<>).MakeGenericType(identity.Type);
            var neighborMethod = neighborInfo.GetMethod("make_property_path", BindingFlags.Static | BindingFlags.NonPublic);
            return (PropertyPath) neighborMethod?.Invoke(null, new object[] {identity.Path});
        }

        #endregion


    }
}