using System;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Attributes {
    static class EntityAttributeHelper {

        [CanBeNull]
        public static EntityAttribute GetConfigurationForRepository(this Type info, Type repositoryType) {

            var attributes = info.GetInfo().GetCustomAttributes<EntityAttribute>().Where(x => x.Repository == null || repositoryType.IsAssignableFrom(x.Repository)).ToList();
            if (attributes.Count == 0)
                return null;

            var cachedAttributes = attributes.OfType<GloballyCachedEntityAttribute>().ToList();
            var cachedAttribute = cachedAttributes.SingleOrDefault(x => repositoryType.IsAssignableFrom(x.Repository)) ?? cachedAttributes.FirstOrDefault();
            if (cachedAttribute != null)
                return cachedAttribute;

            return attributes.SingleOrDefault(x => repositoryType.IsAssignableFrom(x.Repository)) ?? attributes.FirstOrDefault();
        }



    }
}