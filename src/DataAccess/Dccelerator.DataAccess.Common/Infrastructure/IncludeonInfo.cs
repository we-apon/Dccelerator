using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using Dccelerator.Reflection.Abstract;


namespace Dccelerator.DataAccess.Infrastructure {
    class IncludeonInfo : IEntityInfo {
        readonly EntityInfo _entityInfo;
        readonly IncludeChildrenAttribute _includeon;
        readonly PropertyPath _propertyPath;
        readonly IProperty _targetProperty;

        public IncludeonInfo(EntityInfo entityInfo, IncludeChildrenAttribute includeon) {
            _entityInfo = entityInfo;
            _includeon = includeon;

            _propertyPath = _entityInfo.Type.GetPropertyPath(includeon.PropertyName);
            if (_propertyPath == null)
                throw new InvalidOperationException($"Can't find property with PropertyName '{includeon.PropertyName}', specified in {nameof(IncludeChildrenAttribute)} on type {entityInfo.Type}");

            _targetProperty = _propertyPath.GetTargetProperty();


        }




        #region Implementation of IEntityInfo

        public Type Type => _type ?? (_type = IsCollection ? _targetProperty.Info.PropertyType.ElementType() : _targetProperty.Info.PropertyType);
        Type _type;



#if NET40
        public Type TypeInfo => Type;
        public IEntityInfo[] Children { get; } = null;
#else
        public TypeInfo TypeInfo => _typeInfo ?? (_typeInfo = Type.GetInfo());
        TypeInfo _typeInfo;
#endif


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
                    _entityInfo.Type.Name + "Id",
                    _entityInfo.Type.Name + "_Id",
                    _entityInfo.EntityName + "Id",
                    _entityInfo.EntityName + "_Id"
                };
                if (declaringType != null) {
                    possibleNames.Add(declaringType.Name + "Id");
                    possibleNames.Add(declaringType.Name + "_Id");
                }


                var keyId = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => possibleNames.Any(z => string.Compare(x.Name, z, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (keyId != null)
                    return keyId;

                throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                    $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{_includeon.PropertyName}' " +
                                                    $"on entity {_entityInfo.Type}, because it can't be finded automatically.");
            }
            else {
                var possibleNames = new [] {
                    _targetProperty.Info.Name + "Id",
                    _targetProperty.Info.Name + "_Id",                        
                };

                var keyId = _entityInfo.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => possibleNames.Any(z => string.Compare(x.Name, z, StringComparison.InvariantCultureIgnoreCase) == 0));

                if (keyId == null)
                    throw new InvalidOperationException($"You must specify {nameof(IncludeChildrenAttribute.KeyIdName)} in {nameof(IncludeChildrenAttribute)} " +
                                                        $"with {nameof(IncludeChildrenAttribute.PropertyName)} '{_includeon.PropertyName}' " +
                                                        $"on entity {_entityInfo.Type}, because it can't be finded automatically.");

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
        public IDataAccessRepository RealRepository { get; }
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

            var ownerReference = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.PropertyType.IsAssignableFrom(_entityInfo.Type) && x.Name == (_targetProperty.Info.DeclaringType ?? _targetProperty.Info.ReflectedType ?? Type).Name);

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
                return Type.MakeArrayType();

            if (targetType.IsAbstract || targetType.IsInterface)
                return targetType.IsGenericType
                    ? typeof (List<>).MakeGenericType(Type)
                    : typeof (ArrayList);

            return null;
        }


        #endregion



    }
}