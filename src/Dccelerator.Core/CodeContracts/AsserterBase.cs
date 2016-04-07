using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dccelerator.Logging;
using JetBrains.Annotations;


namespace Dccelerator.CodeContracts {
    abstract class AsserterBase<TRequest> : IAssert<TRequest> {
        readonly TRequest _request;
        [CanBeNull] readonly string _parameterName;
        [CanBeNull] readonly ILoggerSession _logger;
        readonly List<Task<string>> _tasks = new List<Task<string>>();
        bool _isDone;
        bool? _result;
        static readonly Type _requestType = typeof (TRequest);

        public string ErrorMessage { get; private set; }


        protected AsserterBase(TRequest request, [CanBeNull] string parameterName, ILoggerSession logger) {
            _logger = logger;
            _request = request;
            _parameterName = String.IsNullOrWhiteSpace(parameterName)
                ? null
                : String.Concat(" ", _parameterName);
        }


        /// <exception cref="Exception">Can't compile/invoke <paramref name="requestExpression"/>.</exception>
        protected AsserterBase(Expression<Func<TRequest>> requestExpression, ILoggerSession logger) {
            _logger = logger;
            try {
                _request = requestExpression.Compile()();
                _parameterName = requestExpression.Path();
            }
            catch (Exception e) {
                logger?.Log(TraceEventType.Error, () => $"Can't get object for assertion throuth expression {requestExpression}.\n\n{e}");
                throw;
            }
        }

            

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose() {
            /*do_nothing()*/
        }


        public IAssert<TRequest> That(Expression<Func<TRequest, bool>> that) {
            if (_isDone)
                return this;

            var task = Task.Factory.StartNew(() => {
                try {
                    var callback = that.Compile();
                    bool result;
                    try {
                        result = callback(_request);
                    }
                    catch {
                        result = false;
                    }

                    if (result)
                        return null;

                    var message = that.ToString();
                    _logger?.Log(TraceEventType.Error, () => $"{typeof (TRequest)}{_parameterName} does not meet the assertion '{message}'", _request);
                    return FormatExpression(message);
                }
                catch (Exception e) {
                    var message = that.ToString();
                    _logger?.Log(TraceEventType.Error, () => $"{typeof (TRequest)}{_parameterName} does not meet the assertion '{message}'\n'n{e}", _request);
                    return FormatExpression(message);
                }
            });

            _tasks.Add(task);
            return this;
        }



        public bool IsValid() {
            string unusedError;
            return IsValid(out unusedError);
        }


        public bool IsValid(out string error) {
            if (_isDone) {
                error = null;
                return _result ?? false;
            }

            var fails = _tasks.Where(x => !String.IsNullOrWhiteSpace(x.Result)).Select(x => x.Result).ToList();
            if (fails.Any()) {
                ErrorMessage = ErrorMessage ?? $"{_requestType.Name} does not meet the assertions:\n\t{String.Join(",\n\t", fails)}";
                error = ErrorMessage;
                _result = false;
            }
            else {
                error = null;
                ErrorMessage = null;
                _result = true;
            }


            _isDone = true;
            return _result.Value;
        }


        string FormatExpression(string expression) {

            var lambdaIndex = expression.IndexOf("=>", StringComparison.Ordinal);
            if (lambdaIndex < 1)
                return expression;

            var parameterName = expression.Substring(0, lambdaIndex).Trim('(', ')', ' ');
            return expression
                .Substring(lambdaIndex + 2)
                .Replace($" {parameterName}.", " ")
                .Replace($"({parameterName}.", "(")
                .Replace($"[{parameterName}.", "[")
                .Replace("(", "( ")
                .Replace(")", " )")
                .Replace("[", " [")
                .Replace("]", " ]");
        }


    }
}