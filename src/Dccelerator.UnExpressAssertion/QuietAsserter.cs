using System;
using System.Linq.Expressions;


namespace Dccelerator.UnExpressAssertion {
    sealed class QuietAsserter<TRequest> : AsserterBase<TRequest> {
        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression" />.</exception>
        public QuietAsserter(Expression<Func<TRequest>> requestExpression, LogAssertionDelegate log) : base(requestExpression, log) { }

        public QuietAsserter(TRequest request, string parameterName, LogAssertionDelegate log) : base(request, parameterName, log) {}
    }
}