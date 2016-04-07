using System;
using System.Linq.Expressions;
using Dccelerator.Logging;


namespace Dccelerator.CodeContracts {
    sealed class QuietAsserter<TRequest> : AsserterBase<TRequest> {
        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression" />.</exception>
        public QuietAsserter(Expression<Func<TRequest>> requestExpression, ILoggerSession logger) : base(requestExpression, logger) { }

        public QuietAsserter(TRequest request, string parameterName, ILoggerSession logger) : base(request, parameterName, logger) {}
    }
}