using System;
using System.Linq.Expressions;


namespace Dccelerator.UnExpressAssertion {
    sealed class ExceptionAsserter<TRequest> : AsserterBase<TRequest> {

        public ExceptionAsserter(TRequest request, string parameterName, LogAssertionDelegate log) : base(request, parameterName, log) {}


        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression" />.</exception>
        public ExceptionAsserter(Expression<Func<TRequest>> requestExpression, LogAssertionDelegate log) : base(requestExpression, log) {
                
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