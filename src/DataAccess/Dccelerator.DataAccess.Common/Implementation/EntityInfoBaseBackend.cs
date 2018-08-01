using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.UnFastReflection;


namespace Dccelerator.DataAccess.Implementation {

    
    public static class EntityInfoBaseBackend {
        
#if NET40
        internal static Dictionary<string, TAttribute> Get<TAttribute>(Type typeInfo) where TAttribute : SecondaryKeyAttribute {
#else
        internal static Dictionary<string, TAttribute> Get<TAttribute>(TypeInfo typeInfo) where TAttribute : SecondaryKeyAttribute {
#endif

            var dict = new Dictionary<string, TAttribute>();

#if (NETSTANDARD1_3)
            var properties = typeInfo.AsType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#else
            var properties = typeInfo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
            var attributeType = RUtils<TAttribute>.Type;

            foreach (var property in properties) {
                var keyAttributes = property.GetCustomAttributes(attributeType, inherit: true).Cast<TAttribute>().ToList();
                if (keyAttributes.Count < 1)
                    continue;

                if (keyAttributes.Count > 1)
                    throw new InvalidOperationException($"Property {typeInfo.FullName}.{property.Name} contains more that one {attributeType.Name}.");

                var attribute = keyAttributes.Single();
                if (string.IsNullOrWhiteSpace(attribute.Name))
                    attribute.Name = property.Name;

                dict.Add(property.Name, attribute);
            }

            return dict;
        }


        /// <summary>
        /// Checks, is property persisted.
        /// <para>Property should contain getter, not be an collection (except of strings and byte[]), enum or any class type, and also not marked with <see cref="NotPersistedAttribute"/>.</para>
        /// </summary>
        /// <param name="prop">An property</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static bool IsPersisted(this PropertyInfo prop) {
            return IsPersistedProperty(prop);
        }




        static readonly Type _notPersistedAttributeType = typeof(NotPersistedAttribute);
        static readonly Type _stringType = typeof(string);
        static readonly Type _byteArrayType = typeof(byte[]);
        static readonly Type _enumerableType = typeof(IEnumerable);




        internal static readonly Func<PropertyInfo, bool> IsPersistedProperty = property => {

            //? if marked with NotPersistedAttribute
            if (property.GetCustomAttributesData().Any(x => x.AttributeType() == _notPersistedAttributeType))
                return false;

            if (!property.CanRead)
                return false;

            var type = property.PropertyType;

            if (type == _stringType || type.IsAssignableFrom(_byteArrayType))
                return true;

            if (_enumerableType.IsAssignableFrom(type) || type.GetInfo().IsClass)
                return false;

            return true;
        };
    }
}