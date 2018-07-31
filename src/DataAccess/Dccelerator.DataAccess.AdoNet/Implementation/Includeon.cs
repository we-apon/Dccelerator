using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;

namespace Dccelerator.DataAccess.Ado.Implementation {

    public class Includeon : IIncludeon {
        readonly IAdoEntityInfo _ownerInfo;
        readonly Func<Type, IAdoEntityInfo> _getInfoCallback;
        readonly IProperty _targetProperty;
        readonly object _lock = new object();

        public Includeon(IncludeChildrenAttribute inclusion, IAdoEntityInfo ownerInfo, Func<Type, IAdoEntityInfo> getInfoCallback) {
            Attribute = inclusion;
            _ownerInfo = ownerInfo;
            _getInfoCallback = getInfoCallback;

            var propertyPath = ownerInfo.EntityType.GetPropertyPath(inclusion.TargetPath);
            if (propertyPath == null)
                throw new InvalidOperationException($"Can't find property with TargetPath '{inclusion.TargetPath}', specified in {nameof(IncludeChildrenAttribute)} on type {ownerInfo.EntityType}");

            _targetProperty = propertyPath.GetTargetProperty();
        }

        public IncludeChildrenAttribute Attribute { get; }

        IEntityInfo IIncludeon.Info => Info;
        public IAdoEntityInfo Info => _info ?? (_info = _getInfoCallback(IsCollection ? _targetProperty.Info.PropertyType.ElementType() : _targetProperty.Info.PropertyType));
        IAdoEntityInfo _info;



        string IIncludeon.TargetPath => Attribute.TargetPath;


        public bool IsCollection => _isCollection ?? (_isCollection = _targetProperty.Info.PropertyType.IsAnCollection()).Value;
        bool? _isCollection;

        public Type TargetCollectionType => _targetCollectionType ?? (_targetCollectionType = GetTargetCollectionType());
        Type _targetCollectionType;




        public string OwnerNavigationReferenceName => _ownerNavigationReferenceName ?? (_ownerNavigationReferenceName = GetOwnerReference());
        string _ownerNavigationReferenceName;


        public string ForeignKeyFromMainEntityToCurrent => _foreignKeyFromMainEntityToCurrent ?? (_foreignKeyFromMainEntityToCurrent = GetForeignKeyFromMainEntityToCurrent());
        string _foreignKeyFromMainEntityToCurrent;


        public string ForeignKeyFromCurrentEntityToMain => _foreignKeyFromCurrentEntityToMain ?? (_foreignKeyFromCurrentEntityToMain = GetForeingKeyFromCurrentEntityToMain());
        string _foreignKeyFromCurrentEntityToMain;





        Type GetTargetCollectionType() {
            if (!IsCollection)
                return null;

            var targetType = _targetProperty.Info.PropertyType;

            var info = targetType.GetInfo();
            if (info.IsAbstract || info.IsInterface) {
                return info.IsGenericType
                    ? typeof(List<>).MakeGenericType(Info.EntityType)
                    : typeof(ArrayList);
            }

            foreach (var ctor in targetType.GetConstructors()) {
                var parameters = ctor.GetParameters();
                if (!parameters.Any() || parameters.All(x => x.IsOptional))
                    return targetType;
            }


            if (targetType.IsArray) // I think this is not needed anymore..
                return Info.EntityType.MakeArrayType();

            return null;
        }


        

        string GetForeingKeyFromCurrentEntityToMain() {
            if (_foreignKeyFromCurrentEntityToMain != null)
                return _foreignKeyFromCurrentEntityToMain;

            lock (_lock) {
                if (_foreignKeyFromCurrentEntityToMain != null)
                    return _foreignKeyFromCurrentEntityToMain;


                if (!string.IsNullOrWhiteSpace(Attribute.KeyIdName))
                    return Attribute.KeyIdName;

                var navigationPropName = (_targetProperty.Info.DeclaringType ?? _ownerInfo.EntityType).Name;
                var name = GetForeignKeyName(Info, _ownerInfo, prop => prop.Name == navigationPropName, navigationPropName);
                if (name != null)
                    return name;

                var msg = $"{nameof(GetForeignKeyFromMainEntityToCurrent)}: Can't find even possible foreign key to main entity '{_ownerInfo.EntityName}' from includeon {Info.EntityName}." +
                          $"So, you need to override search method in used {nameof(IAdoNetRepository)}, or use {nameof(ForeignKeyAttribute)}.";

                Infrastructure.Internal.TraceEvent(TraceEventType.Critical, msg);
                throw new InvalidOperationException(msg);
            }
        }


        string GetForeignKeyFromMainEntityToCurrent() {
            if (_foreignKeyFromCurrentEntityToMain != null)
                return _foreignKeyFromMainEntityToCurrent;

            lock (_lock) {
                if (_foreignKeyFromCurrentEntityToMain != null)
                    return _foreignKeyFromMainEntityToCurrent;

                if (!string.IsNullOrWhiteSpace(Attribute.KeyIdName))
                    return Attribute.KeyIdName;

                var name = GetForeignKeyName(_ownerInfo, Info, null, Attribute.TargetPath);
                if (name != null)
                    return name;

                var msg = $"{nameof(GetForeignKeyFromMainEntityToCurrent)}: Can't find even possible foreign key to includeon '{Info.EntityName}'." +
                          $"So, you need to override search method in used {nameof(IAdoNetRepository)}, or use {nameof(IncludeChildrenAttribute)}.{nameof(IncludeChildrenAttribute.KeyIdName)} or {nameof(ForeignKeyAttribute)}.";

                Infrastructure.Internal.TraceEvent(TraceEventType.Critical, msg);
                throw new InvalidOperationException(msg);
            }
        }


        string GetForeignKeyName(IAdoEntityInfo mainInfo, IAdoEntityInfo foreignInfo, Func<PropertyInfo, bool> subCriteria = null, string navigationPropName = null) {
            if (mainInfo.ForeignKeys.Any()) {
                var fk = mainInfo.ForeignKeys.Values.FirstOrDefault(x => x.Relationship == Relationship.ManyToOne && x.ForeignEntityName == foreignInfo.EntityName);
                if (fk != null)
                    return fk.Name;
            }

            subCriteria = subCriteria ?? (info => true);

            var ownerProps = mainInfo.EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            var foreignNavigationProp = ownerProps.Where(x => x.PropertyType.IsAssignableFrom(foreignInfo.EntityType) && subCriteria(x)).ToArray();
            if (foreignNavigationProp.Length > 1) {
                Infrastructure.Internal.TraceEvent(TraceEventType.Warning, 
                    $"{nameof(GetForeignKeyFromMainEntityToCurrent)}: Founded more than one possible navigation keys for includeon '{foreignInfo.EntityName}'." +
                    "Chosen foreign key can be wrong.");
            }
            else if (foreignNavigationProp.Length == 1) {
                var possibleName = foreignNavigationProp.Single().Name + "Id";
                if (ownerProps.Any(x => x.Name == possibleName))
                    return possibleName;
            }


            var anotherPossibleName = navigationPropName ?? foreignInfo.EntityType.Name + "Id";
            if (ownerProps.Any(x => x.Name == anotherPossibleName))
                return anotherPossibleName;

            return null;
        }


        bool _isOwnerReferenceGetted;
        string GetOwnerReference() {
            if (_isOwnerReferenceGetted)
                return _ownerNavigationReferenceName;

            lock (_lock) {
                if (_isOwnerReferenceGetted)
                    return _ownerNavigationReferenceName;

                var ownerReference = Info.EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.PropertyType.IsAssignableFrom(_ownerInfo.EntityType) && x.Name == (_targetProperty.Info.DeclaringType ?? _ownerInfo.EntityType).Name);

                _isOwnerReferenceGetted = true;
                return ownerReference?.Name;
            }
        }

    }
}