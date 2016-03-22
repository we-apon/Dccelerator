using System;
using System.Diagnostics;
using System.Reflection;
using Dccelerator.Convertion;


namespace Dccelerator.Reflection
{
    class Property<TContext, TType> : MemberBase, IProperty
    {
        MethodDelegate<TContext, TType> _getter;
        ActionDelegate<TContext, TType> _setter;
        public PropertyInfo Info { get; }


        public MethodDelegate<TContext, TType> Getter => _getter ?? (_getter = MakeGetter());



        public ActionDelegate<TContext, TType> Setter => _setter ?? (_setter = MakeSetter());


        public Property(PropertyInfo property) : base(property.Name, MemberKind.Property) {
            Info = property;
        }



        public bool TryGetValue(object context, out object value) {
            try {
                TType val;
                if (Getter.TryInvoke((TContext) context, out val)) {
                    value = val;
                    return true;
                }
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, e.ToString());
            }

            value = null;
            return false;
        }

        

        public bool TrySetValue(object context, object value) {
            try {
                var val = value is TType
                    ? (TType) value
                    : (TType)(value.ConvertTo(typeof (TType)));

                return Setter.TryInvoke((TContext)context, val);
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, e.ToString());
                return false;
            }
        }


        ActionDelegate<TContext, TType> MakeSetter() {
            var method = Info.GetSetMethod(true);
            return method == null ? null : new ActionDelegate<TContext, TType>(method);
        }


        MethodDelegate<TContext, TType> MakeGetter() {
            var method = Info.GetGetMethod(true);
            return method == null ? null : new MethodDelegate<TContext, TType>(method);
        }
        
        
    }

    



}