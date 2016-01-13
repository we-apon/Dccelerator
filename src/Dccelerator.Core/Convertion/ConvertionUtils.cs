using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Dccelerator.Reflection;


namespace Dccelerator.Convertion
{
    /// <summary>
    /// Declarative extensions usable for conversion between <c>objects</c> of different <see cref="Type"/>.
    /// </summary>
    public static class ConvertionUtils
    {

        /// <summary>
        /// Tries to convert <paramref name="value"/> to <typeparamref name="T"/>
        /// </summary>
        /// <param name="value">An value of absolutelly anything type.</param>
        /// <seealso cref="SmartConvert"/>
        /// <seealso cref="FillAllFrom{TEntity, TOtherEntity}"/>
        public static T ConvertTo<T>( this object value) {
            return value.ConvertTo(typeof (T)).SafeCastTo<T>();
        }


        /// <summary>
        /// Tries to convert <paramref name="value"/> to <paramref name="targetType"/>.
        /// </summary>
        /// <param name="value">An value of absolutelly anything type.</param>
        /// <param name="targetType">Any type.</param>
        /// <seealso cref="SmartConvert"/>
        /// <seealso cref="FillAllFrom{TEntity, TOtherEntity}"/>
        
        public static object ConvertTo<T>(this T value,  Type targetType) {
            var targetInfo = targetType.GetInfo();

            if (value == null)
                return targetInfo.IsClass ? null : Activator.CreateInstance(targetType);

            var valueType = value.GetType();
            //? It's necessary to use value.GetType() instead of typeof(T), because often this method will be called on object that was getted through reflection, or something..
           
             
            if (TypeCache.IsAssignableFrom(targetType, valueType))
                return value;

            if (targetType == TypeCache.StringType)
                return value.ToString();

            if (targetInfo.IsEnum) {
                try {
                    return Enum.ToObject(targetType, Convert.ToInt32(value)); //bug: will not work with not numeric enums
                }
                catch (Exception) {
                    return null;
                }
            }

            if (valueType == TypeCache.StringType && TypeCache.IsAssignableFrom(targetType, TypeCache.GuidType))
                return Guid.Parse(value.SafeCastTo<string>());

            if (TypeCache.IsAssignableFrom(TypeCache.ConvertibleType, valueType))
                return Convert.ChangeType(value, targetType, null);

            var collection = value.AsAnCollection(valueType);
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

            return targetList;
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

            var destinationProps = entity.GetType().Properties();

            foreach (var destinationProperty in destinationProps) {
                object value;

                if (!TypeManipulator.TryGetValueOnPath(other, destinationProperty.Key, out value) || !TypeManipulator.TrySetValueOnPath(entity, destinationProperty.Key, value)) {
                    unconvertedProps.Add(destinationProperty.Key, destinationProperty.Value);
                }
            }

            return unconvertedProps;
        }
    }
}