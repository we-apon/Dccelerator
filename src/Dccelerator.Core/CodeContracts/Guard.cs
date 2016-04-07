using System;
using System.Linq.Expressions;
using Dccelerator.Logging;
using JetBrains.Annotations;


namespace Dccelerator.CodeContracts {
    /// <summary>
    /// An method Guardian.
    /// Use it to check method parameters in some fluent way.
    /// </summary>
    /// <seealso cref="IAssert{TRequest}"/>
    public static class Guard {

        #region Get IAsserter

        /// <summary>
        /// Returns an asserter that should be used like:
        /// using (var assert = Guard.GetExceptionAsserter(someRequest) { 
        ///     assert.That(x => x.Document.Type.Id != Guid.Empty)
        ///           .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
        ///           .IsValid() 
        /// }
        /// </summary>
        /// <seealso cref="IAssert{TRequest}"/>
        /// <param name="request">Object that being asserted</param>
        /// <param name="argument">Name of argument of invoker method, that being asserted.</param>
        /// <param name="variable">Name of local variable of invoker method, that being asserted.</param>
        /// <param name="logger">An logger session, to internally log all getted errors</param>
        public static IAssert<TRequest> GetExceptionAsserter<TRequest>(TRequest request, [InvokerParameterName] string argument = null, string variable = null, ILoggerSession logger = null) {
            return new ExceptionAsserter<TRequest>(request, argument ?? variable, logger);
        }


        /// <summary>
        /// Returns an asserter that should be used like:
        /// using (var assert = Guard.GetExceptionAsserter(someRequest) { 
        ///     assert.That(x => x.Document.Type.Id != Guid.Empty)
        ///           .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
        ///           .IsValid() 
        /// }
        /// </summary>
        /// <seealso cref="IAssert{TRequest}"/>
        /// <param name="requestExpression">Expression to get object from invoker context</param>
        /// <param name="logger">An logger session, to internally log all getted errors</param>
        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression" />.</exception>
        public static IAssert<TRequest> GetExceptionAsserter<TRequest>([NotNull]Expression<Func<TRequest>> requestExpression, ILoggerSession logger = null) {
            return new ExceptionAsserter<TRequest>(requestExpression, logger);
        }


        /// <summary>
        /// Returns an asserter that should be used like:
        /// string error;
        /// if (!Guard.AssertOn(someRequest)
        ///        .That(x => x.Document.Type.Id != Guid.Empty)
        ///        .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
        ///        .IsValid(out error)) {
        ///     return error;
        /// }
        /// or like
        /// var assert = Guard.GetAsserter
        /// if (!assert.That(x => x.Document.Type.Id != Guid.Empty)
        ///       .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
        ///       .IsValid() {
        ///     return assert.ErrorMessage;
        /// }
        /// </summary>
        /// <seealso cref="IAssert{TRequest}"/>
        /// <param name="requestExpression">Expression to get object from invoker context</param>
        /// <param name="logger">An logger session, to internally log all getted errors</param>
        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression" />.</exception>
        public static IAssert<TRequest> GetAsserter<TRequest>([NotNull]Expression<Func<TRequest>> requestExpression, ILoggerSession logger = null) {
            return new QuietAsserter<TRequest>(requestExpression, logger);
        }

        /// <summary>
        /// Returns an asserter that should be used like:
        /// string error;
        /// if (!Guard.AssertOn(someRequest)
        ///        .That(x => x.Document.Type.Id != Guid.Empty)
        ///        .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
        ///        .IsValid(out error)) {
        ///     return error;
        /// }
        /// 
        /// 
        /// or like
        /// 
        /// var assert = Guard.GetAsserter
        /// if (!assert.That(x => x.Document.Type.Id != Guid.Empty)
        ///       .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
        ///       .IsValid() {
        ///     return assert.ErrorMessage;
        /// }
        /// 
        /// </summary>
        /// <seealso cref="IAssert{TRequest}"/>
        /// <param name="request">Object that being asserted</param>
        /// <param name="argument">Name of argument of invoker method, that being asserted.</param>
        /// <param name="variable">Name of local variable of invoker method, that being asserted.</param>
        /// <param name="logger">An logger session, to internally log all getted errors</param>
        public static IAssert<TRequest> GetAsserter<TRequest>(TRequest request, [InvokerParameterName] string argument = null, string variable = null, ILoggerSession logger = null) {
            return new QuietAsserter<TRequest>(request, argument ?? variable, logger);
        }

        #endregion


    }
}