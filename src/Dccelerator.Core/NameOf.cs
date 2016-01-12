using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace Dccelerator {
    /// <summary>
    /// Utility usable for getting names of members used in expressions.
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    public static class NameOf<T>
    {
        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TProp>( Expression<Func<T, TProp>> expression) {
            return expression.MemberExpression()?.Member.Name;
        }


        #region Method


        #region Without Parameters

        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member( Expression<Func<T, Action>> expression) {
            return LastMemberNameOf(expression);
        }


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes>( Expression<Func<T, Func<TRes>>> expression) {
            return LastMemberNameOf(expression);
        }

        #endregion


        #region T1


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<T1>( Expression<Func<T, Action<T1>>> expression) {
            return LastMemberNameOf(expression);
        }


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1>( Expression<Func<T, Func<T1, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }

        #endregion


        #region T1, T2


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1, T2>( Expression<Func<T, Action<T1, T2, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1, T2>( Expression<Func<T, Func<T1, T2, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }

        #endregion


        #region T1, T2, T3


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1, T2, T3>( Expression<Func<T, Action<T1, T2, T3, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1, T2, T3>( Expression<Func<T, Func<T1, T2, T3, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }

        #endregion


        #region T1, T2, T3, T4


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1, T2, T3, T4>( Expression<Func<T, Action<T1, T2, T3, T4, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }


        /// <summary>
        /// Returns name of last member in <paramref name="expression"/>.
        /// </summary>
        
        public static string Member<TRes, T1, T2, T3, T4>( Expression<Func<T, Func<T1, T2, T3, T4, TRes>>> expression) {
            return LastMemberNameOf(expression);
        }

        #endregion


        #endregion


        /// <summary>
        /// Returns name of last <paramref name="expression"/> argument.
        /// </summary>
        static string LastMemberNameOf<TIn>( Expression<TIn> expression) {
            var methodCall = (expression.Body as UnaryExpression)?.Operand as MethodCallExpression;
            if (methodCall == null)
                return null;

            var stack = new Stack<MemberInfo>();
            foreach (var info in methodCall.Arguments.OfType<ConstantExpression>().Select(x => x.Value).OfType<MemberInfo>()) {
                stack.Push(info);
            }

            return stack.Pop().Name;
        }
    }
}