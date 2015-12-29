using System;
using System.Collections.Generic;
using System.Reflection;


namespace Dccelerator.Reflection
{
    public class TypeManipulator
    {
        #region public static bool TryGetNestedProperty(object context, string path, out object value)

        public static bool TryGetNestedProperty(object context, string path, out object value) {
            if (context == null || string.IsNullOrWhiteSpace(path)) {
                value = null;
                return false;
            }

            return TryGetNestedProperty(context, new PropIdentity(context.GetType(), path), out value);
        }


        static bool TryGetNestedProperty(object context, PropIdentity identity, out object value) {
            PropertyPath propertyPath;
            bool isGetted;
            lock (_propertyPaths)
                isGetted = _propertyPaths.TryGetValue(identity, out propertyPath);

            if (propertyPath != null)
                return propertyPath.TryGetValueOfTargetProperty(context, out value);

            if (isGetted) {
                value = null;
                return false;
            }


            propertyPath = make_property_path(identity);
            lock (_propertyPaths)
                _propertyPaths[identity] = propertyPath;


            if (propertyPath == null) {
                value = null;
                return false;
            }

            return propertyPath.TryGetValueOfTargetProperty(context, out value);
        }

        #endregion



        #region public static bool TrySetNestedProperty(object context, string path, object value)

        public static bool TrySetNestedProperty(object context, string path, object value) {
            return context != null && !string.IsNullOrWhiteSpace(path) && TrySetNestedProperty(context, new PropIdentity(context.GetType(), path), value);
        }


        static bool TrySetNestedProperty(object context, PropIdentity identity, object value) {

            PropertyPath propertyPath;
            bool isGetted;
            lock (_propertyPaths)
                isGetted = _propertyPaths.TryGetValue(identity, out propertyPath);

            if (propertyPath != null)
                return propertyPath.TrySetTargetProperty(context, value);

            if (isGetted)
                return false;

            propertyPath = make_property_path(identity);
            lock (_propertyPaths)
                _propertyPaths[identity] = propertyPath;

            return propertyPath != null && propertyPath.TrySetTargetProperty(context, value);
        }

        #endregion



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


        static readonly Dictionary<PropIdentity, PropertyPath> _propertyPaths = new Dictionary<PropIdentity, PropertyPath>();


        static PropertyPath make_property_path( PropIdentity identity) {
            var neighborInfo = typeof (TypeManipulator<>).MakeGenericType(identity.Type);
            var neighborMethod = neighborInfo.GetMethod("make_property_path", BindingFlags.Static | BindingFlags.NonPublic);
            return (PropertyPath) neighborMethod?.Invoke(null, new object[] {identity.Path});
        }

        #endregion


    }
}