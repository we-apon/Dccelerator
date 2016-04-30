using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;


namespace Dccelerator.DataAccess.Lazy {
    [Serializable]
    public abstract class LazyDataAccessAttributeBase : LocationInterceptionAspect, IInstanceScopedAspect
    {
        protected abstract SeverityType IsAcceptedMessageSeverityType();


        protected bool IsAccepted(LocationInfo location) {

            if (GetCustomAttribute(location.PropertyInfo, typeof(NotPersistedAttribute)) != null || location.PropertyInfo.GetCustomAttributes(typeof(NotPersistedAttribute), true).Any()) {
                Message.Write(MessageLocation.Of(location), IsAcceptedMessageSeverityType(), "3543", $"'LazyDataAccess' aspect can'n be used on properties marked with {typeof(NotPersistedAttribute).Name}.\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                return false;
            }

            var type = location.LocationType;
            if (typeof (string) == type) {
                Message.Write(MessageLocation.Of(location), IsAcceptedMessageSeverityType(), "3543", $"'LazyDataAccess' aspect can't be used on string properties!\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                return false;
            }

            if (typeof (IEnumerable).IsAssignableFrom(type)) {
                if (type.IsArray)
                    type = type.GetElementType();
                else {
                    var generics = type.GetGenericArguments();
                    if (generics.Length == 0) {
                        Message.Write(MessageLocation.Of(location), IsAcceptedMessageSeverityType(), "3543", $"'LazyDataAccess' aspect can't be used on non-generic collections that implement IList interface!\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                        return false;
                    }

                    type = generics[0];
                }
            }


            if (type.IsPrimitive || !type.IsClass) {
                Message.Write(MessageLocation.Of(location), IsAcceptedMessageSeverityType(), "3543", $"'LazyDataAccess' aspect can only be used on classes!\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                return false;
            }

            if (type.Namespace?.StartsWith("System.") == true) {
                Message.Write(MessageLocation.Of(location), IsAcceptedMessageSeverityType(), "3543", $"'LazyDataAccess' aspect can only be used on custom classes. System classes is unacceptable!\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                return false;
            }
/*
            if (!type.GetCustomAttributes(inherit:true).Any(x => x is EntityAttribute))
                return false;*/

            if (!typeof (LazyEntity).IsAssignableFrom(location.DeclaringType)) {
                Message.Write(MessageLocation.Of(location), IsAcceptedMessageSeverityType(), "3543", $"'LazyDataAccess' aspect can only be used on custom classes implemented ILazyEntity interface.\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                return false;
            }

            //todo: validate lazy link path.
            Message.Write(MessageLocation.Of(location), SeverityType.Verbose, "3543", $"!!! 'LazyDataAccess' are accepted!!!.\nLocation is {location.DeclaringType.FullName}.{location.PropertyInfo.Name}\n");
                
            return true;
        }


        public override void CompileTimeInitialize(LocationInfo targetLocation, AspectInfo aspectInfo) {
            base.CompileTimeInitialize(targetLocation, aspectInfo);

            if (targetLocation.LocationType.IsGenericType) {
                var genericType = targetLocation.LocationType.GetGenericTypeDefinition();
                IsCollection = genericType == typeof (ICollection<>)
                               || genericType == typeof(IEnumerable<>)
                               || genericType == typeof (IList<>)
                               || genericType == typeof (Collection<>)
                               || genericType == typeof (List<>)
                               || genericType == typeof (HashSet<>);

                if (genericType == typeof (IList<>) || genericType == typeof (ICollection<>) || genericType == typeof (IEnumerable<>))
                    TargetCollectionType = typeof (List<>).MakeGenericType(targetLocation.LocationType.GetGenericArguments());
                else if (IsCollection)
                    TargetCollectionType = targetLocation.LocationType;

                var genericArgs = targetLocation.LocationType.GetGenericArguments();
                RealLocationType = genericArgs.Length > 0 ? genericArgs[0] : targetLocation.LocationType;
            }
            else if (targetLocation.LocationType.IsArray) {
                IsArray = true;
                IsCollection = true;
                TargetCollectionType = targetLocation.LocationType;
                RealLocationType = targetLocation.LocationType.GetElementType();
            }
            else {
                IsCollection = typeof (IEnumerable).IsAssignableFrom(targetLocation.LocationType);
                TargetCollectionType = IsCollection
                    ? typeof (List<object>)
                    : null;

                RealLocationType = IsCollection 
                    ? targetLocation.LocationType.ElementType() 
                    : targetLocation.LocationType;
            }


            var link = GetCustomAttribute(targetLocation.PropertyInfo, typeof (ForeignKeyAttribute)) as ForeignKeyAttribute;
            if (link != null)
                CriterionName = link.Name;
            else if (targetLocation.DeclaringType == RealLocationType && IsCollection) {
                var properties = RealLocationType.Properties();
                var sameTypeLazyProperties = properties.Values.Where(x => x.PropertyType.IsAssignableFrom(RealLocationType)).ToList();

                var linkProperties = properties.Values.Where(x => x.GetCustomAttributes(typeof (ForeignKeyAttribute), true).OfType<ForeignKeyAttribute>()
                        .Any(f => sameTypeLazyProperties.Any(z => z.Name == f.NavigationPropertyPath))).ToList();



//                var linkProperties = sameTypeLazyProperties.Where(x => properties.Values.Any(z => {
//                    var foreignKeyAttribute = z
//                        .GetCustomAttributes(typeof (ForeignKeyAttribute), true)
//                        .OfType<ForeignKeyAttribute>()
//                        .SingleOrDefault(f => f.NavigationPropertyPath == x.Name);
//
//                    return foreignKeyAttribute != null;
//                })).ToList();


                if (linkProperties.Any())
                    CriterionName = linkProperties.First().Name;
                else {
                    CriterionName = properties.Values.Select(x => x.Name)
                        .SingleOrDefault(x => x == targetLocation.DeclaringType.Name + "Id" || x == RealLocationType.Name + "Id") ?? "ParentId";
                }

            }
            else if (IsCollection)
                CriterionName = targetLocation.DeclaringType?.Name + "Id";
            else {
                CriterionName = "Id";
            }

        }



        [NotNull]
        object _writeLock;

        /// <summary>
        /// If <see langword="true"/> - prevents getter to loading something from database.
        /// </summary>
        protected bool AlreadyLoaded;


        /// <summary>
        /// Tells, is current location is an collection
        /// </summary>
        protected bool IsCollection;

        /// <summary>
        /// If current location <see cref="IsCollection"/> - returns type of collection's elements, 
        /// otherwise - location declared type.
        /// </summary>
        protected Type RealLocationType;


        /// <summary>
        /// Name of sql parameter.
        /// </summary>
        protected string CriterionName;


        /// <summary>
        /// Tells, is current location are an array.
        /// </summary>
        protected bool IsArray;

        /// <summary>
        /// <see cref="Type"/> that can be used as target location's collection.
        /// </summary>
        protected Type TargetCollectionType;



        public override void OnGetValue(LocationInterceptionArgs args) {
            args.ProceedGetValue();
            if (AlreadyLoaded)
                return;

            if (IsValueAlreadySpecified(args)) {
                AlreadyLoaded = true;
                return;
            }

            var lazyEntity = (LazyEntity) args.Instance;
            if (!lazyEntity.Context.IsLoadingAllowed) {
                if (!IsCollection || args.Value != null)
                    return;
                
                //? then property is collection - we initialize it with empty collection if it now initialized yet, to avoid null reference exceptions
                InitializeCollectionPropertyWithEmptyCollection(args);
                return;
            }


            LazyLoadValueFromDatabase(args, lazyEntity);
        }


        void LazyLoadValueFromDatabase(LocationInterceptionArgs args, LazyEntity lazyEntity) {
            lock (_writeLock) {
                args.ProceedGetValue();

                if (AlreadyLoaded)
                    return;

                if (IsValueAlreadySpecified(args)) {
                    AlreadyLoaded = true;
                    return;
                }

                if (lazyEntity.Read == null) {
                    AlreadyLoaded = true; //?if Read callback is null - it means that entity was created by the client code, and not was getted from db, so it's not persisted at all.
                    return;
                }

                var value = GetValueFor(args, lazyEntity, lazyEntity.Read);
                args.SetNewValue(value);
                AlreadyLoaded = true;
                args.ProceedGetValue();
            }
        }


        void InitializeCollectionPropertyWithEmptyCollection(LocationInterceptionArgs args) {
            lock (_writeLock) {
                args.ProceedGetValue();

                if (AlreadyLoaded)
                    return;

                if (args.Value != null)
                    return;

                var targetCollection = Activator.CreateInstance(TargetCollectionType);
                args.SetNewValue(targetCollection);
                args.ProceedGetValue();
            }
        }


        bool IsValueAlreadySpecified([NotNull] LocationInterceptionArgs args) {
            if (args.Value == null)
                return false;

            return !IsCollection || ((IEnumerable) args.Value).GetEnumerator().MoveNext();
        }


        [CanBeNull]
        object GetValueFor([NotNull] LocationInterceptionArgs args, LazyEntity parent, Func<LazyEntity, Type, ICollection<IDataCriterion>, IEnumerable<object>> read) {
            var criterion = CriterionFor(args);
            if (criterion == null)
                return null;


            
           // var collection = GetLazy.Entity(RealLocationType, (ILazyEntity)args.Instance).By(criterion);
            var collection = read(parent, RealLocationType, new[] {criterion});
            if (IsCollection) {
                var targetCollection = (IList)Activator.CreateInstance(TargetCollectionType);
                foreach (var item in collection)
                    targetCollection.Add(item);

                return targetCollection;
            }

            return collection.FirstOrDefault();
        }


        [CanBeNull]
        private IDataCriterion CriterionFor([NotNull] LocationInterceptionArgs args) {
            object id;
            if (IsCollection) {
                var identifiedEntity = args.Instance as IIdentifiedEntity;

                if (identifiedEntity != null)
                    id = identifiedEntity.Id;
                else if (!args.Instance.TryGetValueOnPath("Id", out id)) {
                    Internal.TraceEvent(TraceEventType.Warning, $"Can't get value of property of not identified entity '{args.Instance.GetType().FullName}.Id'");
                    return null;
                }

                if (id == null || (id is Guid && (Guid) id == Guid.Empty))
                    return null;

                return new DataCriterion {
                    Name = CriterionName,
                    Value = id,
                    Type = id.GetType()
                };
            }

            if (!args.Instance.TryGetValueOnPath(args.Location.PropertyInfo.Name + "Id", out id)) {
                Internal.TraceEvent(TraceEventType.Warning, $"Can't get value of property '{args.Location.PropertyInfo.Name}Id'");
                return null;
            }


            if (id == null || (id is Guid && (Guid) id == Guid.Empty))
                return null;

            return new DataCriterion {
                Name = CriterionName,
                Value = id,
                Type = id.GetType()
            };
        }




        public object CreateInstance(AdviceArgs adviceArgs) {
            return MemberwiseClone();
        }


        public void RuntimeInitializeInstance() {
            _writeLock = new object();
        }
    }
}