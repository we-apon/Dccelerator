using System;


namespace Dccelerator.Reflection
{
    public class PropertyPath
    {
        public IProperty Property { get; set; }
        
        public PropertyPath Nested { get; set; }


        public bool TrySetTargetProperty(object context, object value) {
            var current = this;
            var curContext = context;

            while (current.Nested != null) {
                object curValue;
                if (!current.Property.TryGetValue(curContext, out curValue))
                    return false;

                if (curValue == null) {
                    curValue = Activator.CreateInstance(current.Property.Info.PropertyType);
                    if (!current.Property.TrySetValue(curContext, curValue))
                        return false;
                }

                curContext = curValue;
                current = current.Nested;
            }

            return current.Property.TrySetValue(curContext, value);
        }




        public bool TryGetValueOfTargetProperty(object context, out object value) {
            var current = this;
            var curContext = context;

            while (current.Nested != null) {
                object curValue;
                if (!current.Property.TryGetValue(curContext, out curValue) || curValue == null) {
                    value = null;
                    return false;
                }

                curContext = curValue;
                current = current.Nested;
            }

            return current.Property.TryGetValue(curContext, out value);
        }


        public bool TryGetValueOfTargetProperty<TValue>(object context, out TValue value) {
            var current = this;
            var curContext = context;

            while (current.Nested != null) {
                object curValue;
                if (!current.Property.TryGetValue(curContext, out curValue) || curValue == null) {
                    value = default(TValue);
                    return false;
                }

                curContext = curValue;
                current = current.Nested;
            }

            var typedProp = current.Property as IProperty<TValue>;
            if (typedProp != null)
                return typedProp.TryGetValue(curContext, out value);

            value = default(TValue);
            return false;
        }




        public IProperty GetTargetProperty() {
            var path = this;
            while (path.Nested != null)
                path = path.Nested;

            return path.Property;
        }
    }
}