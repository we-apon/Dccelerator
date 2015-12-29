using System;
using System.Linq;
using Dccelerator.Convertion.Abstract;
using Dccelerator.Reflection;


namespace Dccelerator.Convertion
{
    /// <summary>
    /// Tells to <see cref="SmartConvert"/> utility that marked type can be converted to other types through some kind of <see cref="IConvertHelper"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ConvertibleToAttribute : Attribute, IConvertibleAttribute
    {
        private static readonly Type _convertHelperType = typeof (IConvertHelper);
        private Type _through;

        /// <summary>
        /// Types in that current member can be converted.
        /// </summary>
        public Type[] TargetTypes { get; }

        /// <summary>
        /// <see cref="Type"/> that implemented <see cref="IConvertHelper"/> interface and having an public parameterless constructor.
        /// </summary>
        /// <exception cref="InvalidOperationException" accessor="set"><paramref name="value"/> should be sub-type of <see cref="IConvertHelper"/></exception>
        public Type Through {
            get { return _through; }
            set {
                if (!TypeCache.IsAssignableFrom(_convertHelperType, value))
                    throw new InvalidOperationException($"Property ConvertibleToAttribute.Through should be sub-type of {_convertHelperType}");

                _through = value;
            }
        }


        /// <summary>
        /// Says, is current attribute instance can be used to convert marked <c>object</c> to <paramref name="targetType"/>
        /// </summary>
        public bool IsUsableFor(Type targetType) {
            return TargetTypes.Any(x => TypeCache.IsAssignableFrom(targetType, x)); //bug: it's may be wrong
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetTypes">Types, to that marked with current attribute class can be converted</param>
        public ConvertibleToAttribute( params Type[] targetTypes) {
            TargetTypes = targetTypes;
        }
    }
}