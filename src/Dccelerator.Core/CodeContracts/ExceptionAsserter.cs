using System;
using System.Linq.Expressions;
using Dccelerator.Logging;


namespace Dccelerator.CodeContracts {
    sealed class ExceptionAsserter<TRequest> : AsserterBase<TRequest> {

        public ExceptionAsserter(TRequest request, string parameterName, ILoggerSession logger) : base(request, parameterName, logger) {}


        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression" />.</exception>
        public ExceptionAsserter(Expression<Func<TRequest>> requestExpression, ILoggerSession logger) : base(requestExpression, logger) {
                
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="InvalidOperationException">Than some assertion doesn mot meet.</exception>
        public override void Dispose() {
            if (!IsValid())
                throw new InvalidOperationException(ErrorMessage);
        }
    }
}