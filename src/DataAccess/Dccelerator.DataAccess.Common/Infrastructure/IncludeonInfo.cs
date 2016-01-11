using System;
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

        public string EntityName { get; }


        /// <summary>
        /// Contains name and type of key id field of entity.
        /// Can be null, because syntetic entities may hasn't unique identifier.
        /// </summary>
        public PropertyInfo KeyId { get; }


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



        public string[] ColumnNames { get; }
        public string ChildIdKey { get; }

        public string TargetPath => _includeon.PropertyName;



        public string OwnerReferenceName { get; }
        public Type TargetCollectionType { get; }

        #endregion
    }
}