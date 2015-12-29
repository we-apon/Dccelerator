using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Dccelerator.Reflection.Abstract
{
    public abstract class MethodDelegateBase : MemberBase
    {
        ParameterInfo[] _parameters;
        Type[] _parameterTypes;
        Type[] _delegateParameterTypes;

        Type _declaringType;
        Type _delegateType;


        protected MethodDelegateBase(MethodInfo method) : base(method.Name, MemberKind.Method) {
            Method = method;
        }



        public MethodInfo Method { get; set; }
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



        protected TFunc GetLambda<TFunc>() {
            var type = typeof (TFunc);
            var generic = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();

            var isAction = generic.FullName.StartsWith("System.Action");
            var systemArgsCount = isAction ? 1 : 2;

            var parameters = new ParameterExpression[args.Length - systemArgsCount];
            var callParameters = new Expression[parameters.Length];
            for (var i = 1; i < args.Length - (isAction ? 0 : 1); i++) {
                var j = i - 1;
                parameters[j] = Expression.Parameter(args[i]);
                callParameters[j] = ParameterTypes[j].IsAssignableFrom(args[i])
                    ? (Expression) parameters[j]
                    : Expression.Convert(parameters[j], ParameterTypes[j]);
            }

            var context = Expression.Parameter(args[0]);
            var callContext = DeclaringType.IsAssignableFrom(args[0])
                ? (Expression) context
                : Expression.Convert(context, DeclaringType);




            var lambdaParameters = new ParameterExpression[args.Length - (isAction ? 0 : 1)];
            lambdaParameters[0] = context;
            Array.Copy(parameters, 0, lambdaParameters, 1, parameters.Length);

            try {
                var body = (Expression) Expression.Call(callContext, Method, callParameters);
                if (!isAction) {
                    body = args[args.Length - 1].IsAssignableFrom(Method.ReturnType)
                        ? body
                        : Expression.Convert(body, Method.ReturnType);
                }

                var lambda = Expression.Lambda<TFunc>(body, lambdaParameters).Compile();
                return lambda;
            }
            catch (Exception) {
                return default(TFunc);
            }
        }
    }
}