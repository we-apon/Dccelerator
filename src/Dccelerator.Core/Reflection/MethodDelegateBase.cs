using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dccelerator.Reflection
{
    public abstract class MethodDelegateBase : MemberBase
    {
        ParameterInfo[] _parameters;
        Type[] _parameterTypes;
        Type[] _delegateParameterTypes;

        Type _declaringType;
        Type _delegateType;
        bool? _isAction;


        protected MethodDelegateBase(MethodInfo method) : base(method.Name, MemberKind.Method) {
            Method = method;
        }



        public MethodInfo Method { get; set; }

        public bool IsAction => _isAction ?? (_isAction = Method.ReturnType == typeof(void)).Value;

        public ParameterInfo[] Parameters => _parameters ?? (_parameters = Method.GetParameters());

        public Type[] ParameterTypes => _parameterTypes ?? (_parameterTypes = Parameters.Select(x => x.ParameterType).ToArray());


        public Type[] DelegateParameterTypes {
            get {
                if (_delegateParameterTypes != null)
                    return _delegateParameterTypes;

                _delegateParameterTypes = new Type[ParameterTypes.Length + 2];
                _delegateParameterTypes[0] = DeclaringType;
                Array.Copy(ParameterTypes, 0, _delegateParameterTypes, 1, ParameterTypes.Length);
                _delegateParameterTypes[ParameterTypes.Length + 1] = Method.ReturnType;

                return _delegateParameterTypes;
            }
        }


        public Type DeclaringType => _declaringType ?? (_declaringType = Method.DeclaringType);


        public Type DelegateType => _delegateType ?? (_delegateType = Method.ReturnType == typeof (void)
            ? TypeCache.ActionTypeWith(DelegateParameterTypes.Length - 1).MakeGenericType(DelegateParameterTypes)
            : TypeCache.FuncTypeWith(DelegateParameterTypes.Length - 1).MakeGenericType(DelegateParameterTypes));



        protected TFunc GetDelegate<TFunc>() {
            var delegateArgs = typeof(TFunc).GetGenericArguments();
            var delegateParametersCount = delegateArgs.Length - (IsAction ? 0 : 1);

            if (delegateParametersCount != Parameters.Length + 1) {
                Internal.TraceEvent(TraceEventType.Warning, $"Delegate {typeof(TFunc)} has {delegateParametersCount} parameters, but should has {Parameters.Length + 1} for calling method {DeclaringType.FullName}.{Method.Name}()");
            }


            var delegateParameters = new ParameterExpression[delegateParametersCount];
            var methodParameters = new Expression[Parameters.Length];



            var delegateContext = Expression.Parameter(delegateArgs[0]);
            delegateParameters[0] = delegateContext;

            var methodContext = Method.IsStatic
                ? null
                : (DeclaringType.IsAssignableFrom(delegateArgs[0]) ? (Expression) delegateContext : Expression.Convert(delegateContext, DeclaringType));




            for (var i = 1; i < delegateParametersCount; i++) {
                delegateParameters[i] = Expression.Parameter(delegateArgs[i]);

                var j = i - 1;
                methodParameters[j] = ParameterTypes[j].IsAssignableFrom(delegateArgs[i])
                    ? (Expression) delegateParameters[i]
                    : Expression.Convert(delegateParameters[i], ParameterTypes[j]);
            }

            
            try {
                var methodCall = (Expression) Expression.Call(methodContext, Method, methodParameters);
                if (!IsAction) {
                    methodCall = delegateArgs[delegateArgs.Length - 1].IsAssignableFrom(Method.ReturnType)
                        ? methodCall
                        : Expression.Convert(methodCall, Method.ReturnType);
                }

                return Expression.Lambda<TFunc>(methodCall, delegateParameters).Compile();
            }
            catch (Exception e) {
                Internal.TraceEvent(TraceEventType.Critical, $"Failed generating delegate to method {DeclaringType.FullName}.{Method.Name}()\n\n{e}");
                return default(TFunc);
            }
        }
    }
}