using System;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Attributes {
    static class EntityAttributeHelper {


        [CanBeNull]
        public static EntityAttribute GetConfigurationForRepository(this Type info, Type repositoryType) {

            var attributes = info.GetInfo().GetCustomAttributes<EntityAttribute>().Where(x => repositoryType.IsAssignableFrom(x.Repository)).ToList();
            if (attributes.Count == 0)
                return null;

            var cachedAttribute = attributes.OfType<GloballyCachedEntityAttribute>().SingleOrDefault();
            if (cachedAttribute != null)
                return cachedAttribute;

            return attributes.SingleOrDefault();
        }



    }
}