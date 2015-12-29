using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dccelerator.Convertion.Abstract;
using Dccelerator.Reflection;


namespace Dccelerator.Convertion
{
    /// <summary>
    /// <c>Converter</c> that can be used to convert <c>object</c> of any type to <c>object</c> any other type.
    /// </summary>
    public class SmartConvert
    {
        private static readonly Dictionary<Type, Dictionary<Type, ConvertibleToAttribute>> _convertibleAttributesCache = new Dictionary<Type, Dictionary<Type, ConvertibleToAttribute>>();
        private static readonly Dictionary<Type, Dictionary<Type, ConvertibleFromAttribute>> _convertibleFromAttributesCache = new Dictionary<Type, Dictionary<Type, ConvertibleFromAttribute>>();
        private static readonly Dictionary<Type, Dictionary<Type, IConvertHelper>> _convertHelpers = new Dictionary<Type, Dictionary<Type, IConvertHelper>>();
        private static readonly IConvertHelper _defaultConvertHelper = new DefaultConvertHelper();
        private static readonly object _lock = new object();

        private readonly object _entity;
        private readonly Type _sourceType;
        private readonly Dictionary<Type, ConvertibleToAttribute> _convertibleAttributes;


        private class DefaultConvertHelper : IConvertHelper
        {
            
            public object ConvertTo(Type targetType, object entity) {
                var target = Activator.CreateInstance(targetType);
                target.FillAllFrom(entity);
                return target;
            }
        }


        private SmartConvert( object entity) {
            _entity = entity;
            if (_entity == null)
                return;

            _sourceType = _entity.GetType();
            _convertibleAttributes = GetConvertibleToAttributesFrom(_sourceType);
        }


        /// <summary>
        /// Specifies <paramref name="entity"/> that will be converted
        /// </summary>
        /// <param name="entity">Any object</param>
        
        public static SmartConvert Object( object entity) {
            return new SmartConvert(entity);
        }


        /// <summary>
        /// Returns previously specified <c>object</c> converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <seealso cref="To"/>
        
        public T To<T>() where T : class {
            return To(typeof (T)).SafeCastTo<T>();
        }


        /// <summary>
        /// Specifies <paramref name="targetType"/> of object, converts and returns it.
        /// </summary>
        /// <param name="targetType">Any type</param>
        /// <seealso cref="To{T}"/>
        
        public object To( Type targetType) {
            if (_entity == null)
                return null;

            var converter = ConverterFor(targetType);
            return converter.ConvertTo(targetType, _entity);
        }



        static readonly Type _convertibleToAttributeType = typeof (ConvertibleToAttribute);
        static readonly Type _convertibleFromAttributeType = typeof (ConvertibleFromAttribute);

        
        private static Dictionary<Type, ConvertibleToAttribute> GetConvertibleToAttributesFrom( Type type) {
            Dictionary<Type, ConvertibleToAttribute> attributes;
            lock (_lock) {
                if (_convertibleAttributesCache.TryGetValue(type, out attributes)) {
                    return attributes;
                }


                attributes = new Dictionary<Type, ConvertibleToAttribute>();

                foreach (var attribute in type.GetInfo().GetCustomAttributes().Where(x => TypeCache.IsAssignableFrom(_convertibleToAttributeType, x.GetType())).Cast<ConvertibleToAttribute>()) {
                    
                    foreach (var targetType in attribute.TargetTypes) {
                        attributes.Add(targetType, attribute);
                    }
                }

                _convertibleAttributesCache.Add(type, attributes);
            }
            return attributes;
        }


        static Dictionary<Type, ConvertibleFromAttribute> GetConvertibleFromAttributesOn(Type type) {
            var attributes = new Dictionary<Type, ConvertibleFromAttribute>();

            foreach (var attribute in type.GetInfo().GetCustomAttributes().Where(x => TypeCache.IsAssignableFrom(_convertibleFromAttributeType, x.GetType())).Cast<ConvertibleFromAttribute>()) {
                foreach (var targetType in attribute.TargetTypes) {
                    attributes.Add(targetType, attribute);
                }
            }

            return attributes;
        }



        private IConvertHelper ConverterFor( Type targetType) {
            Dictionary<Type, IConvertHelper> convertHelperAssociation;
            lock (_convertHelpers) {
                if (!_convertHelpers.TryGetValue(_sourceType, out convertHelperAssociation)) {
                    convertHelperAssociation = new Dictionary<Type, IConvertHelper>();
                    _convertHelpers.Add(_sourceType, convertHelperAssociation);
                }
            }

            IConvertHelper helper;
            lock (convertHelperAssociation) {
                if (convertHelperAssociation.TryGetValue(targetType, out helper))
                    return helper;
            }


            ConvertibleToAttribute attribute;
            if (_convertibleAttributes.TryGetValue(targetType, out attribute)) {
                helper = attribute.Through == null
                    ? _defaultConvertHelper
                    : (IConvertHelper) Activator.CreateInstance(attribute.Through);

                lock (convertHelperAssociation) {
                    convertHelperAssociation[targetType] = helper;
                }
                return helper;
            }


            Dictionary<Type, ConvertibleFromAttribute> fromAttributes;
            lock (_lock) {
                if (!_convertibleFromAttributesCache.TryGetValue(targetType, out fromAttributes)) {
                    fromAttributes = GetConvertibleFromAttributesOn(targetType);
                    _convertibleFromAttributesCache.Add(targetType, fromAttributes);
                }
            }

            ConvertibleFromAttribute fromAttribute;
            if (fromAttributes.TryGetValue(_sourceType, out fromAttribute)) {
                helper = fromAttribute.Through == null
                    ? _defaultConvertHelper
                    : (IConvertHelper) Activator.CreateInstance(fromAttribute.Through);
            }
            else {
                helper = _defaultConvertHelper;
            }

            lock (convertHelperAssociation) {
                convertHelperAssociation[targetType] = helper;
            }

            return helper;
        }
    }
}