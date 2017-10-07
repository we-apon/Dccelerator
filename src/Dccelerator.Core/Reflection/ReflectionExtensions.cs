using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dccelerator.Convertion;
using JetBrains.Annotations;


namespace Dccelerator.Reflection {
    /// <summary>
    /// Declarative extensions for interacting with <see cref="System.Reflection"/> api.
    /// </summary>
    public static class ReflectionExtensions {

        

        /// <summary>
        /// Extension to get reflected type of property.
        /// On NET40-NET46 platforms returns it, on netstandard - returns null.
        /// <para>It usable for multiplatform programming, to avoid #if #else statements</para>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [CanBeNull]
        public static Type ReflectedType(this PropertyInfo info) {
#if NET40 || NET45
            return info.ReflectedType;
#else
            return null;
#endif
        }



        /// <summary>
        /// Returns true if can get <paramref name="value"/> from <paramref name="property"/> in passed <paramref name="context"/>.
        /// </summary>
        /// <param name="property">Property defined in <paramref name="context"/>'s type.</param>
        /// <param name="context">Context of <paramref name="property"/></param>
        /// <param name="index">Index of <paramref name="property"/>. Can be <see langword="null"/></param>
        /// <param name="value">Value of <paramref name="property"/></param>
        [Obsolete("Use RUtils to manipulate properties")]
        public static bool TryGetValue(this PropertyInfo property, object context, object[] index, out object value) {
            try {
                value = property.GetValue(context, index);
                return true;
            }
            catch (Exception) {
                value = null;
                return false;
            }
        }


        /// <summary>
        /// Returns true if can get <paramref name="value"/> from <paramref name="property"/> in passed <paramref name="context"/>.
        /// </summary>
        /// <param name="property">Property defined in <paramref name="context"/>'s type.</param>
        /// <param name="context">Context of <paramref name="property"/></param>
        /// <param name="value">Value of <paramref name="property"/></param>
        [Obsolete("Use RUtils to manipulate properties")]
        public static bool TryGetValue(this PropertyInfo property, object context, out object value) {
            return property.TryGetValue(context, null, out value);
        }


        /// <summary>
        /// Returns true if can get <paramref name="value"/> from <see langword="static"/> <paramref name="property"/>.
        /// </summary>
        /// <param name="property">An static property</param>
        /// <param name="value">Value of <paramref name="property"/></param>
        [Obsolete("Use RUtils to manipulate properties")]
        public static bool TryGetValue(this PropertyInfo property, out object value) {
            return property.TryGetValue(null, null, out value);
        }


        /// <summary>
        /// Returns true if can get <paramref name="value"/> from <paramref name="property"/> in passed <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="property">Property defined in <paramref name="context"/>'s type.</param>
        /// <param name="context">Context of <paramref name="property"/></param>
        /// <param name="index">Index of <paramref name="property"/>. Can be <see langword="null"/></param>
        /// <param name="value">Value of <paramref name="property"/></param>
        /// <seealso cref="CastingUtils.SafeCastTo{T}"/>
        [Obsolete("Use RUtils to manipulate properties")]
        public static bool TryGetValue<T>(this PropertyInfo property, object context, object[] index, out T value) {
            try {
                value = property.GetValue(context, index).SafeCastTo<T>();
                return true;
            }
            catch (Exception) {
                value = default(T);
                return false;
            }
        }


        /// <summary>
        /// Returns true if can get <paramref name="value"/> from <paramref name="property"/> in passed <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="property">Property defined in <paramref name="context"/>'s type.</param>
        /// <param name="context">Context of <paramref name="property"/></param>
        /// <param name="value">Value of <paramref name="property"/></param>
        /// <seealso cref="CastingUtils.SafeCastTo{T}"/>
        [Obsolete("Use RUtils to manipulate properties")]
        public static bool TryGetValue<T>(this PropertyInfo property, object context, out T value) {
            return property.TryGetValue(context, null, out value);
        }




        /// <summary>
        /// Returns true if can get <paramref name="value"/> from <see langword="static"/> <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="property">An static property</param>
        /// <param name="value">Value of <paramref name="property"/></param>
        [Obsolete("Use RUtils to manipulate properties")]
        public static bool TryGetValue<T>(this PropertyInfo property, out T value) {
            return property.TryGetValue(null, null, out value);
        }


        /// <summary>
        /// Returns value from <paramref name="property"/> in passed <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="T">Type of returned value</typeparam>
        /// <param name="property">Property defined in <paramref name="context"/>'s type.</param>
        /// <param name="context">Context of <paramref name="property"/></param>
        /// <param name="index">Index of <paramref name="property"/>. Can be <see langword="null"/>. Argument is not required</param>
        /// <seealso cref="CastingUtils.SafeCastTo{T}"/>
        [Obsolete("Use RUtils to manipulate properties")]
        public static T GetValue<T>(this PropertyInfo property, object context = null, object[] index = null) {
            return property != null ? property.GetValue(context, index).SafeCastTo<T>() : default(T);
        }


        /// <summary>
        /// Sets <paramref name="value"/> into <paramref name="targetProperty"/> on passed object.
        /// <paramref name="value"/> is converted to type of <paramref name="targetProperty"/> before it's set's.
        /// This method is part of recursion of methods <see cref="ConvertionUtils.ConvertTo"/> and <see cref="ConvertionUtils.FillAllFrom{T1, T2}"/>, and class <see cref="SmartConvert"/>
        /// </summary>
        /// <exception cref="Exception">Than convertion is not work.</exception>
        [Obsolete("Use RUtils to manipulate properties")]
        public static void SmartSetValue(this PropertyInfo targetProperty, object obj, object value) {
            var targetValue = value.ConvertTo(targetProperty.PropertyType);

            //? намеренно оставлен потенциальный эксепшон, дабы выявить недостающие правила преобразований
            targetProperty.SetValue(obj, targetValue ?? value, null);
        }


        
    }

}