using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Dccelerator.UnFastReflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Infrastructure {
    static class EntityAttributeHelper {

        [CanBeNull]
        public static EntityAttribute GetConfigurationForRepository(this Type info, Type targetRepositoryType) {
            var attributes = info.GetInfo().GetCustomAttributes<EntityAttribute>().Where(x => x.Repository == null || x.Repository.IsAssignableFrom(targetRepositoryType)).ToList();
            if (attributes.Count == 0)
                return null;

            if (attributes.Count == 1)
                return attributes.First();

            var cachedAttributes = attributes.OfType<GloballyCachedEntityAttribute>().ToList();
            if (cachedAttributes.Count == 1)
                return cachedAttributes.First();

            var cachedAttribute = cachedAttributes.MostApplicableAttributeFor(targetRepositoryType) ?? cachedAttributes.FirstOrDefault();
            if (cachedAttribute != null)
                return cachedAttribute;

            return attributes.MostApplicableAttributeFor(targetRepositoryType) ?? attributes.FirstOrDefault();
        }


        [CanBeNull]
        static TAttribute MostApplicableAttributeFor<TAttribute>(this IEnumerable<TAttribute> attributes, Type targetRepositoryType) where TAttribute : EntityAttribute {
            return attributes.Where(x => x.Repository?.IsAssignableFrom(targetRepositoryType) == true)
                .OrderBy(x => x.Repository.GetGenerationNumberTo(targetRepositoryType)) // tries to find most applicable attribute
                .FirstOrDefault();
        }


    }
}