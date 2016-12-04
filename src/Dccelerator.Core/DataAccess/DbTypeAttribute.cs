using System;
using System.Collections.Generic;
using System.Linq;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess {

    public static class DbTypeAttributeExtensions {

        /// <summary>
        /// Returns <see cref="DbTypeAttribute"/> that is most applicable to <paramref name="targetRepositoryType"/>.
        /// </summary>
        [CanBeNull]
        public static DbTypeAttribute MostApplicableToRepository(this IEnumerable<DbTypeAttribute> collection, Type targetRepositoryType) {
            var possible = new List<DbTypeAttribute>();

            foreach (var attribute in collection) {
                if (attribute.RepositoryType == targetRepositoryType)
                    return attribute;

                if (attribute.RepositoryType.GetInfo().IsAssignableFrom(targetRepositoryType.GetInfo()))
                    possible.Add(attribute);
            }

            switch (possible.Count) {
                case 0:
                    return null;

                case 1:
                    return possible.First();

                default:
                    return possible.OrderBy(x => x.RepositoryType.GetGenerationNumberTo(targetRepositoryType)).First();
            }
        }

    }


    public class DbTypeAttribute : Attribute {

        public object DbTypeName { get; set; }

        public Type RepositoryType { get; set; }

        public DbTypeAttribute(object dbTypeName, Type repositoryType) {
            DbTypeName = dbTypeName;
            RepositoryType = repositoryType;
        }
    }
}