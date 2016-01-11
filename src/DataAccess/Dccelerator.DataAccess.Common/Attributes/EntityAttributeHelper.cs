using System;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Attributes {
    static class EntityAttributeHelper {

#if NET40
        public static EntityAttribute GetEntityAttribute(this Type info) {
#else
        public static EntityAttribute GetEntityAttribute(this TypeInfo info) {
#endif
            var attributes = info.GetCustomAttributes<EntityAttribute>().ToList();
            if (attributes.Count == 0)
                return null;

            var cachedAttribute = attributes.OfType<GloballyCachedEntityAttribute>().SingleOrDefault();
            if (cachedAttribute != null)
                return cachedAttribute;

            return attributes.Single();
        }
    }
}