using System;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Attributes {
    static class EntityAttributeHelper {


        [CanBeNull]
#if NET40
        public static EntityAttribute GetConfigurationForRepository(this Type info, Type repositoryType) {
#else
        public static EntityAttribute GetEntityAttribute(this TypeInfo info, Type repositoryType) {
#endif
            var attributes = info.GetCustomAttributes<EntityAttribute>().Where(x => repositoryType.IsAssignableFrom(x.Repository)).ToList();
            if (attributes.Count == 0)
                return null;

            var cachedAttribute = attributes.OfType<GloballyCachedEntityAttribute>().SingleOrDefault();
            if (cachedAttribute != null)
                return cachedAttribute;

            return attributes.SingleOrDefault();
        }



    }
}