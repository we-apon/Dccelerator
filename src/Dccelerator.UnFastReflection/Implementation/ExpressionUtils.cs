﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Dccelerator.UnFastReflection
{
    /// <summary>
    /// Declarative extensions for interacting with <see cref="Expression"/>s.
    /// </summary>
    public static class ExpressionUtils
    {
        /// <summary>
        /// Returns collection of <see cref="MemberExpression"/>s, gatted from <see cref="Expression"/>'s Body or Body.Operand fields.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="expressions">Collection of some expressions</param>
        /// <seealso cref="MemberExpression{T}"/>
        
        public static IEnumerable<MemberExpression> MemberExpressions<T>( this IEnumerable<Expression<T>> expressions) {
            return expressions.Select(x => x.MemberExpression());
        }


        /// <summary>
        /// Returns <paramref name="expression.Body"/> or <paramref name="expression.Body.Operand"/> as <see cref="MemberExpression"/>.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="expression">An <see cref="Expression"/></param>
        
        public static MemberExpression MemberExpression<T>( this Expression<T> expression) {
            return expression.Body as MemberExpression ?? ((UnaryExpression) expression.Body).Operand as MemberExpression;
        }


        /// <summary>
        /// Returns path listed in <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <typeparam name="TOut">Any type</typeparam>
        /// <param name="entity">Any object</param>
        /// <param name="expression">Any expression</param>
        /// <seealso cref="Path{TIn, TOut}"/>
        /// <seealso cref="NameOf{TIn, TOut}"/>
        
        public static string PathTo<T, TOut>(this T entity,  Expression<Func<T, TOut>> expression) {
            return Path(expression);
        }
        

        /// <summary>
        /// Returns path listed in <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <typeparam name="TOut">Any type</typeparam>
        /// <param name="expression">Any expression</param>
        /// <seealso cref="PathTo{T,TOut}"/>
        /// <seealso cref="NameOf{TIn, TOut}"/>
        
        public static string Path(this Expression expression) {
            var builder = new StringBuilder();

            while (expression != null) {
                if (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked) {
                    var unaryExpression = expression as UnaryExpression;
                    expression = unaryExpression?.Operand as MemberExpression;
                    continue;
                }

                if (expression.NodeType == ExpressionType.MemberAccess) {
                    var memberExpression = (MemberExpression) expression;
                    if (memberExpression.Expression == null)
                        break;

                    builder.Insert(0, memberExpression.Member.Name).Insert(0, '.');
                    expression = memberExpression.Expression;
                    continue;
                }
                
                if (expression.NodeType == ExpressionType.ArrayIndex) {
                    var binExpression = (BinaryExpression) expression;
                    builder.Insert(0, ']').Insert(0, binExpression.Right).Insert(0, "[");
                    expression = binExpression.Left;
                    continue;
                }

                //todo: add handlers for all node types

                if (expression.NodeType == ExpressionType.Lambda) {
                    var lambda = (LambdaExpression) expression;
                    expression = lambda.Body;
                    continue;
                }

                break;
            }

            return builder.ToString().Trim('.', ' ');
        }

        


        /// <summary>
        /// Returns name of last element in <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <typeparam name="TOut">Any type</typeparam>
        /// <param name="item">Any object</param>
        /// <param name="expression">Any expression</param>
        /// <seealso cref="PathTo{T,TOut}"/>
        /// <seealso cref="NameOf{TIn, TOut}"/>
        
        public static string NameOf<T, TOut>(this T item,  Expression<Func<T, TOut>> expression) {
            return LastMemberNameOf(expression);
        }


        /// <summary>
        /// Returns name of last <paramref name="expression"/> argument.
        /// </summary>
        static string LastMemberNameOf<TIn>(Expression<TIn> expression) {
            if (!((expression.Body as UnaryExpression)?.Operand is MethodCallExpression methodCall))
                return null;

            var stack = new Stack<MemberInfo>();
            foreach (var info in methodCall.Arguments.OfType<ConstantExpression>().Select(x => x.Value).OfType<MemberInfo>()) {
                stack.Push(info);
            }

            return stack.Pop().Name;
        }
    }
}