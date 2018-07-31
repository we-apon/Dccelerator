using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;


namespace Dccelerator.Reflection
{
    public class PropertyPath {
        public IProperty Property { get; set; }

        public PropertyPath Nested { get; set; }


        public void Build() {
            Get = GetGetDelegate();
            Set = GetSetDelegate();
        }


        IProperty _targetProperty;


        public IProperty GetTargetProperty() {
            if (_targetProperty != null)
                return _targetProperty;

            var path = this;
            while (path.Nested != null)
                path = path.Nested;

            Interlocked.Exchange(ref _targetProperty, path.Property);
            return _targetProperty;
        }


        public Func<object, object> Get;

        public Action<object, object> Set;


        Action<object, object> GetSetDelegate() {
            var delegateContext = Expression.Parameter(typeof(object), "context__param");
            var value = Expression.Parameter(typeof(object), "value");

            var methodContext = Expression.Convert(delegateContext, Property.Info.DeclaringType);

            if (Nested == null) {
                var pr = Expression.Property(methodContext, Property.Info);

                var assi = Property.Info.PropertyType == typeof(object)
                    ? Expression.Assign(pr, value)
                    : Expression.Assign(pr, Expression.Convert(value, Property.Info.PropertyType));

                return Expression.Lambda<Action<object, object>>(assi, delegateContext, value).Compile();
            }


            var list = new List<Expression>();
            var blockParams = new List<ParameterExpression>();

            var contextVar = Expression.Variable(Property.Info.DeclaringType, "context");

            blockParams.Add(contextVar);
            list.Add(Expression.Assign(contextVar, methodContext));

            var prop = Expression.Property(contextVar, Property.Info);

            { // if (prop == null) prop = new Type();
                var nullCheck = Expression.Equal(prop, Expression.Constant(null, typeof(object)));
                var ctor = Property.Info.PropertyType.GetConstructors().Single(x => !x.GetParameters().Any());

                list.Add(Expression.IfThen(nullCheck, Expression.Assign(prop, Expression.New(ctor))));
            }


            var info = Property.Info;
            var nested = this;

            var idx = 1;

            while ((nested = nested.Nested) != null) {
                info = nested.Property.Info;
                prop = Expression.Property(prop, info);

                if (nested.Nested != null) { // if (prop == null) prop = new Type();

                    var nullCheck = Expression.Equal(prop, Expression.Constant(null, typeof(object)));
                    var ctor = info.PropertyType.GetConstructors().Single(x => !x.GetParameters().Any());

                    list.Add(Expression.IfThen(nullCheck, Expression.Assign(prop, Expression.New(ctor))));
                }
            }


            var assign = info.PropertyType == typeof(object)
                ? Expression.Assign(prop, value)
                : Expression.Assign(prop, Expression.Convert(value, info.PropertyType));

            list.Add(assign);
            var block = Expression.Block(blockParams.ToArray(), list.ToArray());


            return Expression.Lambda<Action<object, object>>(block, delegateContext, value).Compile();
        }


        Func<object, object> GetGetDelegate() {

            var delegateContext = Expression.Parameter(typeof(object), "context__param");

            var methodContext = Expression.Convert(delegateContext, Property.Info.DeclaringType);

            if (Nested == null) {
                var pr = (Expression)Expression.Property(methodContext, Property.Info);
                if (!Property.Info.PropertyType.GetInfo().IsClass)
                    return Expression.Lambda<Func<object, object>>(Expression.Convert(pr, typeof(object)), delegateContext).Compile();

                return Expression.Lambda<Func<object, object>>(pr, delegateContext).Compile();
            }


            var list = new List<Expression>();
            var blockParams = new List<ParameterExpression>();

            var contextVar = Expression.Variable(Property.Info.DeclaringType, "context");

            blockParams.Add(contextVar);
            list.Add(Expression.Assign(contextVar, methodContext));

            var prop = Expression.Property(contextVar, Property.Info);

            var variable = Expression.Variable(Property.Info.PropertyType, Property.Info.Name + "__");
            blockParams.Add(variable);
            list.Add(Expression.Assign(variable, prop));

            var returnLabel = Expression.Label(typeof(object));
            var nullLabelExpression = Expression.Label(returnLabel, Expression.Constant(null, typeof(object)));


            { // if (prop == null) prop = new Type();
                var nullCheck = Expression.Equal(variable, Expression.Constant(null, typeof(object)));

                list.Add(Expression.IfThen(nullCheck, Expression.Return(returnLabel, Expression.Constant(null))));
            }


            var info = Property.Info;
            var nested = this;

            var idx = 1;

            while ((nested = nested.Nested) != null) {
                info = nested.Property.Info;
                prop = Expression.Property(variable, info);

                if (nested.Nested == null) {
                    if (nested.Property.Info.PropertyType.GetInfo().IsClass) {
                        list.Add(Expression.Return(returnLabel, prop, info.PropertyType));
                        break;
                    }

                    list.Add(Expression.Return(returnLabel, Expression.Convert(prop, typeof(object)), info.PropertyType));
                    break;
                }

                 // if (prop == null) prop = new Type();

                variable = Expression.Variable(info.PropertyType, info.Name + "__" + idx++);
                blockParams.Add(variable);
                list.Add(Expression.Assign(variable, prop));

                var nullCheck = Expression.Equal(variable, Expression.Constant(null, typeof(object)));
                list.Add(Expression.IfThen(nullCheck, Expression.Return(returnLabel, Expression.Constant(null))));
            
            }

            list.Add(nullLabelExpression);
            var block = Expression.Block(blockParams.ToArray(), list.ToArray());

            return Expression.Lambda<Func<object, object>>(block, delegateContext).Compile();


            /*
            var delegateContext = Expression.Parameter(typeof(object));

            var methodContext = Expression.Convert(delegateContext, Property.Info.DeclaringType);
            var prop = Expression.Property(methodContext, Property.Info);

            var returnNull = Expression.Return(Expression.Label(), Expression.Constant(null, typeof(object)));

            ConditionalExpression condition = null;

            var assign = Expression.Assign(Expression.Variable(Property.Info.PropertyType), prop);

            var nested = this;
            while ((nested = nested.Nested) != null) {
                var nullCheck = Expression.NotEqual(assign, Expression.Constant(null, typeof(object)));

                prop = Expression.Property((Expression)condition ?? assign, nested.Property.Info);

                if (nested.Nested == null) {
                    var cast = Expression.Convert(prop, typeof(object));
                    condition = Expression.IfThenElse(nullCheck, cast, returnNull);
                }
                else {
                    condition = Expression.IfThenElse(nullCheck, prop, returnNull);
                }


                assign = Expression.Assign(Expression.Variable(nested.Property.Info.PropertyType), prop);
            }


            return Expression.Lambda<Func<object, object>>((Expression)condition ?? prop, delegateContext).Compile();
            */
        }




    }

}