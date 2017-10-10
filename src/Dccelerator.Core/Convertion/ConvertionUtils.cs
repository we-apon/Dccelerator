using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Dccelerator.Reflection;
using JetBrains.Annotations;


namespace Dccelerator.Convertion
{
    /// <summary>
    /// Declarative extensions usable for conversion between <c>objects</c> of different <see cref="Type"/>.
    /// </summary>
    public static class ConvertionUtils
    {
        class ConvertionRule {
            MethodInfo _castMethod;
            readonly object _lock = new object();

            public Func<Type, Type, object, ConvertionRule, object> Convert { get; set; }


            public Func<Type, Type, object, ConvertionRule, bool> CanConvert { get; set; }


            public Action<Type, Type, ConvertionRule> PrepareConvertion { get; set; } = (type, type1, arg3) => { /*do_nothing()*/ };


            public Type TargetItemType { get; set; }



            [CanBeNull]
            public MethodInfo CastMethod {
                get { return _castMethod; }
                set {
                    if (_castMethod == null) {
                        lock (_lock) {
                            if (_castMethod == null)
                                _castMethod = value;
                        }
                    }
                }
            }


            public ConvertionRule Clone() {
                return (ConvertionRule)MemberwiseClone();
            }
        }
        
        static ConvertionRule GetRuleFor(Type sourceType, Type targetType, object value) {
            var key = string.Concat(sourceType.FullName, targetType.FullName);

            ConvertionRule rule;
            if (_desidedRules.TryGetValue(key, out rule))
                return rule;

            rule = _allRules.Select(x => x.Clone())
                .Perform(x => x.PrepareConvertion(sourceType, targetType, x))
                .FirstOrDefault(x => x.CanConvert(sourceType, targetType, value, x));
            

            if (rule != null) {
                if (!_desidedRules.TryAdd(key, rule))
                    rule = _desidedRules[key];
            }

            return rule;
        }



        static readonly ConcurrentDictionary<string, ConvertionRule> _desidedRules = new ConcurrentDictionary<string, ConvertionRule>();
        static readonly ConvertionRule[] _allRules = {
            new ConvertionRule {
                CanConvert = (source, target, value, thisRule) => target == TypeCache.StringType,
                Convert = (source, target, value, thisRule) => value.ToString() 
            },

            new ConvertionRule {
                CanConvert = (source, target, value, thisRule) => TypeCache.IsAssignableFrom(target, source),
                Convert = (source, target, value, thisRule) => value
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.CastMethod = source.GetMethod("op_Implicit", new[] { target }),
                CanConvert = (source, target, value, thisRule) => thisRule.CastMethod != null,
                Convert = (source, target, value, thisRule) => thisRule.CastMethod.Invoke(null, new[] {value})
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.CastMethod = target.GetMethod("op_Implicit", new[] { source }),
                CanConvert = (source, target, value, thisRule) => thisRule.CastMethod != null,
                Convert = (source, target, value, thisRule) => thisRule.CastMethod.Invoke(null, new[] {value})
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.CastMethod = source.GetMethod("op_Explicit", new[] { target }),
                CanConvert = (source, target, value, thisRule) => thisRule.CastMethod != null,
                Convert = (source, target, value, thisRule) => thisRule.CastMethod.Invoke(null, new[] {value})
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.CastMethod = target.GetMethod("op_Explicit", new[] { source }),
                CanConvert = (source, target, value, thisRule) => thisRule.CastMethod != null,
                Convert = (source, target, value, thisRule) => thisRule.CastMethod.Invoke(null, new[] {value})
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.CastMethod = target.GetMethod("op_Implicit", new[] {TypeCache.StringType}),
                CanConvert = (source, target, value, thisRule) => value.ToString() != source.FullName && thisRule.CastMethod != null,
                Convert = (source, target, value, thisRule) => thisRule.CastMethod.Invoke(null, new[] {value.ToString()})
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.CastMethod = target.GetMethod("op_Explicit", new[] {TypeCache.StringType}),
                CanConvert = (source, target, value, thisRule) => value.ToString() != source.FullName && thisRule.CastMethod != null,
                Convert = (source, target, value, thisRule) => thisRule.CastMethod.Invoke(null, new[] {value.ToString()})
            },


            new ConvertionRule {
                CanConvert = (source, target, value, thisRule) => target.GetInfo().IsEnum,
                Convert = (source, target, value, thisRule) => {
                    try {
                        return Enum.ToObject(target, Convert.ToInt32(value));
                    }
                    catch (Exception e) {
                        Internal.TraceEvent(TraceEventType.Error, $"{target} is an enum, but convertion from  {source} is failed.\n{e}");
                        return null;
                    }
                }
            },

            new ConvertionRule {
                CanConvert = (source, target, value, thisRule) => source == TypeCache.StringType && TypeCache.IsAssignableFrom(target, TypeCache.GuidType),
                Convert = (source, target, value, thisRule) => {
                    Guid guid;
                    if (Guid.TryParse(value as string, out guid))
                        return guid;

                    Internal.TraceEvent(TraceEventType.Warning, $"Can't parse Guid from {value}");
                    return null;
                }
            },

            new ConvertionRule {
                CanConvert = (source, target, value, thisRule) => source == TypeCache.StringType && TypeCache.IsAssignableFrom(target, TypeCache.DateTimeType),
                Convert = (source, target, value, thisRule) => {
                    var stringValue = (string)value;

                    DateTime date;
                    if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        return date;

                    if (DateTime.TryParse(stringValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
                        return date;


                    if (DateTime.TryParse(stringValue, CultureInfo.CurrentUICulture, DateTimeStyles.None, out date))
                        return date;

#if !(NET_CORE_APP || NET_STANDARD)
                    if (DateTime.TryParse(stringValue, CultureInfo.InstalledUICulture, DateTimeStyles.None, out date))
                        return date;
#endif
                    if (stringValue.Count(x => x == '/') == 2) {
#if !(NET_CORE_APP || NET_STANDARD)
                        if (DateTime.TryParse(stringValue, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out date))
                            return date;
#endif

                        if (DateTime.TryParse(stringValue.Replace('/', '.'), CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                            return date;
                    }
                    

                    Internal.TraceEvent(TraceEventType.Warning, $"Can't parse DateTime from string '{stringValue}'");
                    return null;
                }
            },

            new ConvertionRule {
                CanConvert = (source, target, value, thisRule) => {
                    if (!TypeCache.IsAssignableFrom(TypeCache.ConvertibleType, source))
                        return false;

                    try {
                        return Convert.ChangeType(value, target, null) != null; //? it will return true when no exceptions will be thrown
                    }
                    catch (Exception) {
                        return false;
                    }
                },
                Convert = (source, target, value, thisRule) => {
                    try {
                        return Convert.ChangeType(value, target, null);
                    }
                    catch (Exception e) {
                        Internal.TraceEvent(TraceEventType.Error, $"{source} is an {TypeCache.ConvertibleType}, but changing it's type to {target} fails.\n{e}");
                        return null;
                    }
                }
            },

            new ConvertionRule { 
                CanConvert = (source, target, value, thisRule) => value.AsAnCollection() == null || !target.IsAnCollection(),
                Convert = (source, target, value, thisRule) => SmartConvert.Object(value).To(target)
            },


            new ConvertionRule { 
                PrepareConvertion = (source, target, thisRule) => thisRule.TargetItemType = target.IsArray ? target.ElementType() : null,
                CanConvert = (source, target, value, thisRule) => target.IsArray && value.AsAnCollection() != null,
                Convert = (source, target, value, thisRule) => {

                    var collection = value as ICollection ?? value.AsAnCollection().Cast<object>().ToList();
                    IList targetArray = Array.CreateInstance(thisRule.TargetItemType, collection.Count);

                    var i = 0;
                    foreach (var item in collection)
                        targetArray[i++] = item.ConvertTo(thisRule.TargetItemType);

                    return targetArray;
                }
            },

            new ConvertionRule {
                PrepareConvertion = (source, target, thisRule) => thisRule.TargetItemType = target.IsAnCollection() ? target.ElementType() : null,
                CanConvert = (source, target, value, thisRule) => !target.IsArray && target.IsAnCollection() && value.AsAnCollection() != null,
                Convert = (source, target, value, thisRule) => {

                    var collection = value.AsAnCollection();
                    var targetList = (IList) Activator.CreateInstance(target);
                    foreach (var item in collection)
                        targetList.Add(item.ConvertTo(thisRule.TargetItemType));

                    return targetList;
                }
            },
        };



        
        /// <summary>
        /// Tries to convert <paramref name="value"/> to <typeparamref name="T"/>
        /// </summary>
        /// <param name="value">An value of absolutelly anything type.</param>
        /// <seealso cref="SmartConvert"/>
        /// <seealso cref="FillAllFrom{TEntity, TOtherEntity}"/>
        public static T ConvertTo<T>(this object value) {
            return value.ConvertTo(typeof (T)).SafeCastTo<T>();
        }


        /// <summary>
        /// Tries to convert <paramref name="value"/> to <paramref name="targetType"/>.
        /// </summary>
        /// <param name="value">An value of absolutelly anything type.</param>
        /// <param name="targetType">Any type.</param>
        /// <seealso cref="SmartConvert"/>
        /// <seealso cref="FillAllFrom{TEntity, TOtherEntity}"/>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static object ConvertTo<T>(this T value,  Type targetType) {
            if (value == null)
                return targetType.GetInfo().IsClass ? null : Activator.CreateInstance(targetType);

            var sourceType = value.GetType();
            //? It's necessary to use value.GetType() instead of typeof(T), because often this method will be called on object that was getted through reflection, or something..


            var rule = GetRuleFor(sourceType, targetType, value);
            if (rule != null)
                return rule.Convert(sourceType, targetType, value, rule);

            Internal.TraceEvent(TraceEventType.Error, $"Can't find rule for converting {sourceType} into {targetType}");
            return null;

/*
            if (targetType == TypeCache.StringType)
                return value.ToString();
             

            if (TypeCache.IsAssignableFrom(targetType, sourceType))
                return value;



            var implicitCastMethod = sourceType.GetMethod("op_Implicit", new[] { targetType });
            if (implicitCastMethod != null)
                return implicitCastMethod.Invoke(null, new object[] { value });

            implicitCastMethod = targetType.GetMethod("op_Implicit", new[] { sourceType });
            if (implicitCastMethod != null)
                return implicitCastMethod.Invoke(null, new object[] { value });




            var explicitCastMethod = sourceType.GetMethod("op_Explicit", new[] { targetType });
            if (explicitCastMethod != null)
                return explicitCastMethod.Invoke(null, new object[] { value });

            explicitCastMethod = targetType.GetMethod("op_Explicit", new[] { sourceType });
            if (explicitCastMethod != null)
                return explicitCastMethod.Invoke(null, new object[] { value });


            var stringValue = value.ToString();
            if (stringValue != sourceType.FullName) {

                var implicitCastFromStringMethod = targetType.GetMethod("op_Implicit", new[] {TypeCache.StringType});
                if (implicitCastFromStringMethod != null)
                    return implicitCastFromStringMethod.Invoke(null, new object[] { stringValue });


                var explicitCastFromStringMethod = targetType.GetMethod("op_Explicit", new[] {TypeCache.StringType});
                if (explicitCastFromStringMethod != null)
                    return explicitCastFromStringMethod.Invoke(null, new object[] { stringValue });
            }


            if (targetInfo.IsEnum) {
                try {
                    return Enum.ToObject(targetType, Convert.ToInt32(value)); //bug: will not work with not numeric enums
                }
                catch (Exception) {
                    return null;
                }
            }

            if (sourceType == TypeCache.StringType && TypeCache.IsAssignableFrom(targetType, TypeCache.GuidType))
                return Guid.Parse(value.SafeCastTo<string>());


            if (TypeCache.IsAssignableFrom(TypeCache.ConvertibleType, sourceType))
                return Convert.ChangeType(value, targetType, null);


            var collection = value.AsAnCollection(sourceType);
            if (collection == null || !targetType.IsAnCollection())
                //? I think any IEnumerable can be setted from IList
                return SmartConvert.Object(value).To(targetType);


            IList targetList;

            var targetItemType = targetType.ElementType();

            if (targetType.IsArray) {
                var count = collection.Count(); //bug: need to check it for multipple enumerations
                targetList = Array.CreateInstance(targetItemType, count);

                var i = 0;
                foreach (var item in collection)
                    targetList[i++] = item.ConvertTo(targetItemType);
            }
            else {
                targetList = (IList) Activator.CreateInstance(targetType);
                foreach (var item in collection)
                    targetList.Add(item.ConvertTo(targetItemType));
            }

            return targetList;*/
        }


        /// <summary>
        /// Fills properties of <paramref name="entity"/> from properties of <paramref name="other"/> entity.
        /// If <paramref name="entity"/> or <paramref name="other"/> is null - nothing will be happened.
        /// Exceptions will be silently catched.
        /// </summary>
        /// <seealso cref="SmartConvert"/>
        
        public static Dictionary<string, PropertyInfo> FillAllFrom<TEntity, TOtherEntity>( this TEntity entity,  TOtherEntity other)
            where TEntity : class
            where TOtherEntity : class {
            var unconvertedProps = new Dictionary<string, PropertyInfo>();

            if (entity == null || other == null) {
                return entity?.GetType().Properties() ?? unconvertedProps;
            }

            var sourceProps = other.GetType().Properties();
            var destinationProps = entity.GetType().Properties().Where(x => sourceProps.ContainsKey(x.Key));


            foreach (var destinationProperty in destinationProps) {
                object value;

                if (!other.TryGetValueOnPath(destinationProperty.Key, out value) || !entity.TrySetValueOnPath(destinationProperty.Key, value)) {
                    unconvertedProps.Add(destinationProperty.Key, destinationProperty.Value);
                }
            }

            return unconvertedProps;
        }
    }
}