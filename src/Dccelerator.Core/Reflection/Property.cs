using System;
using System.Diagnostics;
using System.Reflection;
using Dccelerator.Convertion;


namespace Dccelerator.Reflection {
    class Property<TContext, TType> : MemberBase, IProperty<TContext, TType> {

        IFunctionDelegate<TContext, TType> _getter;
        IActionDelegate<TContext, TType> _setter;


        public PropertyInfo Info { get; }


        public IFunctionDelegate<TContext, TType> Getter => _getter ?? (_getter = MakeGetter());



        public IActionDelegate<TContext, TType> Setter => _setter ?? (_setter = MakeSetter());


        public Property(PropertyInfo property) : base(property.Name, MemberKind.Property) {
            Info = property;
        }


        public object GetValue(object context) => Getter.Delegate((TContext)context);


        public bool TryGetValue(object context, out object value) {
            TType val;
            var result = TryGetValue((TContext) context, out val);
            value = val;
            return result;
        }


        public void SetValue(object context, object value) => Setter.Delegate((TContext)context, (TType)value);



        public bool TryGetValue(object context, out TType value) {
            return TryGetValue((TContext) context, out value);
        }


        public bool TryGetValue(TContext context, out TType value) {
            return Getter.TryInvoke(context, out value);
        }


        public bool TrySetValue(object context, object value) {
            try {
                var val = value is TType type
                    ? type
                    : (TType)(value.ConvertTo(typeof (TType)));

                return Setter.TryInvoke((TContext)context, val);
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Error, e.ToString());
                return false;
            }
        }


        public bool TrySetValue(TContext context, TType value) {
            return Setter.TryInvoke(context, value);
        }


        IActionDelegate<TContext, TType> MakeSetter() {
            var method = Info.GetSetMethod(true);
            if (method != null)
                return new ActionDelegate<TContext, TType>(method);

            Internal.TraceEvent(TraceEventType.Warning, $"Property {(Info.ReflectedType() ?? Info.DeclaringType)?.FullName}.{Info.Name} has not setter, but {nameof(TrySetValue)} extension invoked on it!");
            return new NotExistedDelegate();
        }


        IFunctionDelegate<TContext, TType> MakeGetter() {
            var method = Info.GetGetMethod(true);
            if (method != null)
                return new FunctionDelegate<TContext, TType>(method);

            Internal.TraceEvent(TraceEventType.Warning, $"Property {(Info.ReflectedType() ?? Info.DeclaringType)?.FullName}.{Info.Name} has not getter, but {nameof(TryGetValue)} extension invoked on it!");
            return new NotExistedDelegate();
        }
        
        
        /// <summary>
        /// Used then property doesn't contain getter or setter, to avoid exceptions and getter/setter searching or every get/set call)
        /// </summary>
        class NotExistedDelegate : IActionDelegate<TContext, TType>, IFunctionDelegate<TContext, TType> {

            Func<TContext, TType> IFunctionDelegate<TContext, TType>.Delegate => null;


            public bool TryInvoke(TContext context, out TType result) {
                result = default(TType);
                return false;
            }


            Action<TContext, TType> IActionDelegate<TContext, TType>.Delegate => null;


            public bool TryInvoke(TContext context, TType p1) {
                return false;
            }
        }
    }

    



}