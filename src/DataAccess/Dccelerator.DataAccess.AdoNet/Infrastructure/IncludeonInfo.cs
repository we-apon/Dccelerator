using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using Dccelerator.Reflection.Abstract;


namespace Dccelerator.DataAccess.Infrastructure {
/*

    class IncludeonInfo : IEntityInfo {
        readonly EntityInfo _entityInfo;
        readonly IncludeChildrenAttribute _includeon;
        readonly PropertyPath _propertyPath;
        readonly IProperty _targetProperty;

        public IncludeonInfo(EntityInfo entityInfo, IncludeChildrenAttribute includeon) {
            _entityInfo = entityInfo;
            _includeon = includeon;

            _propertyPath = _entityInfo.EntityType.GetPropertyPath(includeon.PropertyName);
            if (_propertyPath == null)
                throw new InvalidOperationException($"Can't find property with PropertyName '{includeon.PropertyName}', specified in {nameof(IncludeChildrenAttribute)} on type {entityInfo.EntityType}");

            _targetProperty = _propertyPath.GetTargetProperty();


        }




        #region Implementation of IEntityInfo

        public Type EntityType => _type ?? (_type = IsCollection ? _targetProperty.Info.PropertyType.ElementType() : _targetProperty.Info.PropertyType);
        Type _type;



#if NET40
        public Type TypeInfo => EntityType;
#else
        public TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = Type.GetInfo());
        TypeInfo _typeInfo;
#endif
        public IEntityInfo[] Children { get; } = null;


        public string EntityName { get; }


        /// <summary>
        /// Contains name and type of key id field of entity.
        /// Can be null, because syntetic entities may hasn't unique identifier.
        /// </summary>
        public PropertyInfo KeyId => _keyId ?? (_keyId = GetKeyId());
        PropertyInfo _keyId;

        PropertyInfo GetKeyId() {
            if (string.IsNullOrWhiteSpace(_includeon.KeyIdName))
                return null;

            if (IsCollection) {
                var declaringType = _targetProperty.Info.DeclaringType ?? _targetProperty.Info.ReflectedType;

                var possibleNames = new List<string>(declaringType == null ? 4 : 6) {
                    _entityInfo.EntityType.Name + "Id",
                    _entityInfo.EntityType.Name + "_Id",
                    _entityInfo.EntityName + "Id",
                    _entityInfo.EntityName + "_Id"
                };
                if (declaringType != null) {
                    possibleNames.Add(declaringType.Name + "Id");
                    possibleNames.Add(declaringType.Name + "_Id");
                }


                var keyId = EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => possibleNames.Any(z => string.Compare(x.Name, z, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (keyId != null)
                    return keyId;

                throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                    $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{_includeon.PropertyName}' " +
                                                    $"on entity {_entityInfo.EntityType}, because it can't be finded automatically.");
            }
            else {
                var possibleNames = new [] {
                    _targetProperty.Info.Name + "Id",
                    _targetProperty.Info.Name + "_Id",                        
                };

                var keyId = _entityInfo.EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => possibleNames.Any(z => string.Compare(x.Name, z, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (keyId == null)
                    throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                        $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{_includeon.PropertyName}' " +
                                                        $"on entity {_entityInfo.EntityType}, because it can't be finded automatically.");

                return keyId;
            }
        }




        /// <summary>
        /// Contains pairs of names and types of all persisted fields
        /// </summary>
        public Tuple<string, Type>[] PersistedFields { get; }
        public bool AllowCache { get; }
        public TimeSpan CacheTimeout { get; }
        public Type RealRepositoryType { get; }
        public IInternalReadingRepository GlobalReadingRepository { get; }
        public IAdoNetRepository RealRepository { get; }
        public EntityAttribute EntityAttribute { get; }



        public bool IsCollection => _isCollection ?? (_isCollection = _targetProperty.Info.PropertyType.IsAnCollection()).Value;
        bool? _isCollection;


        public string[] ColumnNames { get; set; }


        public string ChildIdKey => KeyId?.Name;

        public string TargetPath => _includeon.PropertyName;



        public string OwnerReferenceName => _ownerReferenceName ?? (_ownerReferenceName = GetOwnerReference());
        bool _isOwnerReferenceGetted;
        string _ownerReferenceName;

        string GetOwnerReference() {
            if (_isOwnerReferenceGetted)
                return _ownerReferenceName;

            var ownerReference = EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.PropertyType.IsAssignableFrom(_entityInfo.EntityType) && x.Name == (_targetProperty.Info.DeclaringType ?? _targetProperty.Info.ReflectedType ?? EntityType).Name);

            _isOwnerReferenceGetted = true;
            return ownerReference?.Name;
        }





        public Type TargetCollectionType => _targetCollectionType ?? (_targetCollectionType = GetTargetCollectionType());
        Type _targetCollectionType;

        Type GetTargetCollectionType() {
            if (!IsCollection)
                return null;

            var targetType = _targetProperty.Info.PropertyType;

            if (targetType.IsArray)
                return EntityType.MakeArrayType();

            if (targetType.IsAbstract || targetType.IsInterface)
                return targetType.IsGenericType
                    ? typeof (List<>).MakeGenericType(EntityType)
                    : typeof (ArrayList);

            return null;
        }


        #endregion



    }
*/


}