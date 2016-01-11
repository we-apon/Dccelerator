using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Implementations.ReadingRepositories;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Infrastructure {
    class ConfigurationOf<TEntity> where TEntity: class {

        // ReSharper disable StaticMemberInGenericType
        private static readonly ConfigurationOfEntity _configuration = new ConfigurationOfEntity(TypeManipulator<TEntity>.Type);
        // ReSharper restore StaticMemberInGenericType

        public static EntityInfo Info => _configuration.Info;

        public static IInternalReadingRepository GetReadingRepository() {
            return _configuration.Info.AllowCache
                ? new CachedReadingRepository()
                : new DirectReadingRepository();
        }
    }


    class ConfigurationOfEntity {
        private static readonly ConcurrentDictionary<Type, EntityInfo> _infoCache = new ConcurrentDictionary<Type, EntityInfo>(); 

        public EntityInfo Info { get; }



        public ConfigurationOfEntity(Type entityType) {
            EntityInfo info;
            if (!_infoCache.TryGetValue(entityType, out info)) {
                info = GetInfo(entityType);
                if (!_infoCache.TryAdd(entityType, info))
                    info = _infoCache[entityType];
            }
            Info = info;
        }



        static readonly Type _stringType = typeof(string);
        static readonly Type _byteArrayType = typeof(byte[]);
        static readonly Type _enumerableType = typeof(IEnumerable);


        EntityInfo GetInfo(Type entityType) {
            var info = new EntityInfo {
                Type = entityType,
            };


            //bug: detect and use entity attribute defined on top of entity inheritance hierarchy

            var cachedAttribute = Attribute.GetCustomAttribute(entityType, typeof (GloballyCachedEntityAttribute), true).SafeCastTo<GloballyCachedEntityAttribute>();

            if (cachedAttribute != null) {
                info.AllowCache = true;
                info.CacheTimeout = cachedAttribute.Timeout;
            }
            else {
                info.CacheTimeout = TimeSpan.Zero;
            }

            var entityAttribute = cachedAttribute ?? Attribute.GetCustomAttribute(entityType, typeof (EntityAttribute), true).SafeCastTo<EntityAttribute>();
            if (entityAttribute == null)
                throw new InvalidOperationException("You must mark " + entityType + " with " + typeof (EntityAttribute) + " in order to use it with Get<T> class.");

            info.RealRepositoryType = entityAttribute.Repository;
            info.RealRepository = (IDataAccessRepository) Activator.CreateInstance(info.RealRepositoryType);

            info.GlobalReadingRepository = info.AllowCache ? new CachedReadingRepository() : new DirectReadingRepository();
            info.EntityName = entityAttribute.Name ?? entityType.Name;


            info.PersistedFields = entityType.GetProperties()
                .Where(x => x.CanRead
                            && !x.IsDefined<NotPersistedAttribute>()
                            && (x.PropertyType.IsAssignableFrom(_stringType) || _byteArrayType.IsAssignableFrom(x.PropertyType)
                                || (!_enumerableType.IsAssignableFrom(x.PropertyType) && !x.PropertyType.IsClass)))
                .Select(x => new Tuple<string, Type>(x.Name, x.PropertyType))
                .ToArray();


            if (!string.IsNullOrWhiteSpace(entityAttribute.IdProperty)) {
                var idProperty = entityType.GetProperty(entityAttribute.IdProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (idProperty == null)
                    throw new InvalidOperationException($"Can't find id property, that is specified in {nameof(EntityAttribute)}.{nameof(EntityAttribute.IdProperty)} on entity {entityType}");

                info.KeyId = idProperty;
            }



            var includeonAttributes = Attribute.GetCustomAttributes(entityType, typeof (IncludeChildrenAttribute), false).Cast<IncludeChildrenAttribute>().OrderBy(x => x.ResultSetIndex).ToArray();

            if (info.KeyId == null) {
                var possibleLongName = entityType.Name + "Id";
                var idProperty = info.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x => x.Name == "Id" || x.Name == possibleLongName);
                if (idProperty != null)
                    info.KeyId = idProperty;
                else {
                    if (includeonAttributes.Length == 0)
                        Internal.TraceEvent(TraceEventType.Warning,
                            $"Key Id name of entity {entityType} is not found. " +
                            "Some abilities, like inclusions and lazy data access, may not work properly with this entity. " +
                            $"You can specify {nameof(EntityAttribute.IdProperty)} in {nameof(EntityAttribute)}, " +
                            $"or use identifier in property named 'Id' or '{possibleLongName}'.");
                    else
                        throw new InvalidOperationException($"In order to use inclusions possibility, you must specify {nameof(EntityAttribute.IdProperty)} in {nameof(EntityAttribute)} " +
                                                            $"on entity {info.Type}, or use identifier in property named 'Id' or '{possibleLongName}'.");
                }
            }

            if (includeonAttributes.Length == 0)
                return info;

            info.Children = new EntityInfo[includeonAttributes.Length];
            for (var i = 0; i < includeonAttributes.Length; i++) {
                var includeon = includeonAttributes[i];

                var property = info.Type.GetProperty(includeon.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property == null)
                    throw new InvalidOperationException($"Can't find property with PropertyName '{includeon.PropertyName}', specified in {nameof(IncludeChildrenAttribute)} on type {info.Type}");

                var childInfo = new EntityInfo {
                    TargetPath = includeon.PropertyName,
                    IsCollection = typeof (IEnumerable).IsAssignableFrom(property.PropertyType),
                };

                if (!string.IsNullOrWhiteSpace(includeon.KeyIdName)) {
                    var idProperty = property.PropertyType.GetProperty(includeon.KeyIdName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (idProperty == null)
                        throw new InvalidOperationException($"Can't find id property, that is specified in {nameof(EntityAttribute)}.{nameof(EntityAttribute.IdProperty)} on entity {entityType}");

                    info.KeyId = idProperty;
                }



                if (childInfo.IsCollection) {
                    childInfo.Type = property.PropertyType.ElementType();
                    if (property.PropertyType.IsArray) {
                        childInfo.TargetCollectionType = childInfo.Type.MakeArrayType();
                    }
                    else if (property.PropertyType.IsAbstract || property.PropertyType.IsInterface) {
                        childInfo.TargetCollectionType = property.PropertyType.IsGenericType
                            ? typeof (List<>).MakeGenericType(childInfo.Type)
                            : typeof (ArrayList);
                    }
                    else {
                        childInfo.TargetCollectionType = property.PropertyType;
                    }
                }
                else {
                    childInfo.Type = property.PropertyType;
                }

                childInfo.Type = childInfo.IsCollection
                    ? property.PropertyType.ElementType()
                    : property.PropertyType;


                if (childInfo.KeyId == null) {
                    if (childInfo.IsCollection) {
                        var possibleName = (property.DeclaringType ?? property.ReflectedType ?? info.Type).Name + "Id";

                        var idProperty = childInfo.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x => x.Name == possibleName);
                        if (idProperty != null) {
                            childInfo.KeyId = idProperty;
                        } else throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                                $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{includeon.PropertyName}' " +
                                                                $"on entity {info.Type}, because it can't be finded automatically.");
                    }
                    else {
                        var possibleName = property.Name + "Id";
                        if (info.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(x => x.Name == possibleName)) {
                            childInfo.ChildIdKey = possibleName;
                        } else throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                                $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{includeon.PropertyName}' " +
                                                                $"on entity {info.Type}, because it can't be finded automatically.");

                        var idProperty = childInfo.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x => x.Name == "Id" || x.Name == possibleName);
                        if (idProperty != null)
                            childInfo.KeyId = idProperty;
                        else throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                                $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{includeon.PropertyName}' " +
                                                                $"on entity {info.Type}, because it can't be finded automatically.");
                    }
                }

                var ownerReferenceProperty = childInfo.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x => x.PropertyType.IsAssignableFrom(entityType) && x.Name == (property.DeclaringType ?? property.ReflectedType ?? info.Type).Name);
                childInfo.OwnerReferenceName = ownerReferenceProperty?.Name;

                info.Children[i] = childInfo;
            }

            return info;
        }
    }
}