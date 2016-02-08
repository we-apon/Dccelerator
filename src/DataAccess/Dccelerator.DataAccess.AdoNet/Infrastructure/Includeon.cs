using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using Dccelerator.Reflection.Abstract;


namespace Dccelerator.DataAccess.Ado.Infrastructure {
    public class Includeon {
        readonly IAdoEntityInfo _ownerInfo;
        readonly IProperty _targetProperty;

        public Includeon(IncludeChildrenAttribute inclusion, IAdoEntityInfo ownerInfo) {
            Attribute = inclusion;
            _ownerInfo = ownerInfo;

            var propertyPath = ownerInfo.EntityType.GetPropertyPath(inclusion.TargetPath);
            if (propertyPath == null)
                throw new InvalidOperationException($"Can't find property with TargetPath '{inclusion.TargetPath}', specified in {nameof(IncludeChildrenAttribute)} on type {ownerInfo.EntityType}");

            _targetProperty = propertyPath.GetTargetProperty();
        }

        public IncludeChildrenAttribute Attribute { get; }


        public IAdoEntityInfo Info => _info ?? (_info = new AdoEntityInfo(IsCollection ? _targetProperty.Info.PropertyType.ElementType() : _targetProperty.Info.PropertyType));
        IAdoEntityInfo _info;


        public bool IsCollection => _isCollection ?? (_isCollection = _targetProperty.Info.PropertyType.IsAnCollection()).Value;
        bool? _isCollection;

        public Type TargetCollectionType => _targetCollectionType ?? (_targetCollectionType = GetTargetCollectionType());
        Type _targetCollectionType;




        public string OwnerNavigationReferenceName => _ownerNavigationReferenceName ?? (_ownerNavigationReferenceName = GetOwnerReference());
        string _ownerNavigationReferenceName;




        Type GetTargetCollectionType() {
            if (!IsCollection)
                return null;

            var targetType = _targetProperty.Info.PropertyType;

            if (targetType.IsArray)
                return Info.EntityType.MakeArrayType();

            if (targetType.IsAbstract || targetType.IsInterface)
                return targetType.IsGenericType
                    ? typeof (List<>).MakeGenericType(Info.EntityType)
                    : typeof (ArrayList);

            return null;
        }



        bool _isOwnerReferenceGetted;
        string GetOwnerReference() {
            if (_isOwnerReferenceGetted)
                return _ownerNavigationReferenceName;

            var ownerReference = Info.EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.PropertyType.IsAssignableFrom(_ownerInfo.EntityType) && x.Name == (_targetProperty.Info.DeclaringType ?? _targetProperty.Info.ReflectedType ?? _ownerInfo.EntityType).Name);

            _isOwnerReferenceGetted = true;
            return ownerReference?.Name;
        }

    }
}