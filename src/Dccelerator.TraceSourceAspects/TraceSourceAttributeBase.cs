
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using PostSharp.Aspects;

namespace Dccelerator.TraceSourceAttributes {

    [Serializable]
    public abstract class TraceSourceAttributeBase : OnMethodBoundaryAspect {

        static readonly TraceSource _globalTrace = new TraceSource((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name);
        static readonly ThreadLocal<Stack<Guid>> _parentActicities = new ThreadLocal<Stack<Guid>>(() => new Stack<Guid>());
        
        protected virtual TraceSource Tracer => _trace ?? (_trace = string.IsNullOrWhiteSpace(SourceName) ? _globalTrace : new TraceSource(SourceName));

        [NonSerialized]
        TraceSource _trace;

        protected virtual ParameterInfo[] Parameters { get; set; }
        

        public virtual string SourceName { get; set; }

        public virtual string MethodName { get; set; }

        public virtual bool LogCollectionItems { get; set; }

        public virtual string LogicalOperation { get; set; }

        public virtual int TransferToId { get; set; } = 6000;
        public virtual int TransferBackId { get; set; } = 6010;
        public virtual int StartId { get; set; } = 1000;
        public virtual int StopId { get; set; } = 8000;
        public virtual int ExceptionId { get; set; } = 9900;

        public virtual TraceEventType ExceptionsLevel { get; set; } = TraceEventType.Critical;
        


        protected virtual Guid GetNewActivityGuid(MethodExecutionArgs args) => Guid.NewGuid();

        protected virtual string FormatTransferTo(MethodExecutionArgs args) =>  $"Transfered to {MethodName}";
        protected virtual string FormatTransferBack(MethodExecutionArgs args) => "Transfered back";

        protected virtual string FormatStart(MethodExecutionArgs args) => $"{MethodName}{FormatInputArguments(args)}";
        protected virtual string FormatStop(MethodExecutionArgs args) =>  $"{MethodName}{FormatReturnValue(args.ReturnValue)}";

        protected virtual string FormatException(MethodExecutionArgs args) => args.Exception.ToString();
        protected virtual string FormatLogicalOperation(MethodExecutionArgs args) => LogicalOperation;
        

        protected virtual string FormatCompileTimeMethodName(MethodBase method, AspectInfo aspectInfo) {
            return $"{(method.ReflectedType ?? method.DeclaringType)?.Name}.{method.Name}";
        }

        protected virtual string FormatCompileTimeLogicalOperation(MethodBase method, AspectInfo aspectInfo) {
            return $"{(method.ReflectedType ?? method.DeclaringType)?.Name}.{method.Name}";
        }



        protected virtual string FormatValue(object value, bool logCollectionItems = false, bool inCollection = false, bool printTypeNames = false) {
            IEnumerable collection;

            if (!logCollectionItems || (collection = AsAnCollection(value)) == null)
                return printTypeNames 
                    ? $"{value?.GetType().Name}: \"{value?.ToString() ?? "null"}\""
                    : value?.ToString() ?? "null";
            

            var separator = inCollection ? "\n\t\t" : "\n\t";

            var builder = new StringBuilder(printTypeNames ? value.GetType().Name : value.ToString()).Append(" {");
            foreach (var item in collection) {
                builder.Append(separator).Append(item).Append(", ");
            }
            return builder.Remove(builder.Length - 2, 2).Append(inCollection ? "\n\t" : "\n").Append("}").ToString();
        }


        protected virtual string FormatReturnValue(object value) {
            return value == null ? null : $"Returns {FormatValue(value, LogCollectionItems, printTypeNames:true)}";
        }


        protected string FormatInputArguments(MethodExecutionArgs args) {
            if (args.Arguments.Count == 0)
                return null;

            if (args.Arguments.Count == 1) {
                var parameter = Parameters[0];
                return $"( {parameter.ParameterType.Name} {parameter.Name} = {FormatValue(args.Arguments[0], LogCollectionItems)} )";
            }

            var builder = new StringBuilder("( ");
            for (int i = 0; i < Parameters.Length; i++) {
                if (i >= args.Arguments.Count)
                    break;

                var parameter = Parameters[i];
                builder.Append("\n\t").Append(parameter.ParameterType.Name).Append(" ").Append(parameter.Name)
                    .Append(" = ").Append(FormatValue(args.Arguments[i], LogCollectionItems, inCollection:true))
                    .Append(", ");
            }
            return builder.Remove(builder.Length - 2, 2).Append(" \n)").ToString();
        }



        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo) {
            base.CompileTimeInitialize(method, aspectInfo);
            MethodName = FormatCompileTimeMethodName(method, aspectInfo);
            Parameters = method.GetParameters();
            LogicalOperation = FormatCompileTimeLogicalOperation(method, aspectInfo);
        }


        public override void OnEntry(MethodExecutionArgs args) {
            base.OnEntry(args);

            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
                Trace.CorrelationManager.ActivityId = GetNewActivityGuid(args);
            else {
                _parentActicities.Value.Push(Trace.CorrelationManager.ActivityId);

                var newActivityId = GetNewActivityGuid(args);
                Tracer.TraceTransfer(TransferToId, FormatTransferTo(args), newActivityId);
                Trace.CorrelationManager.ActivityId = newActivityId;
            }
            
            Trace.CorrelationManager.StartLogicalOperation(FormatLogicalOperation(args));
            Tracer.TraceEvent(TraceEventType.Start, StartId, FormatStart(args));
        }


        public override void OnException(MethodExecutionArgs args) {
            base.OnException(args);
            _trace.TraceEvent(ExceptionsLevel, ExceptionId, FormatException(args));
        }


        public override void OnExit(MethodExecutionArgs args) {
            base.OnExit(args);

            var parentActivityStack = _parentActicities.Value;
            var hasParent = parentActivityStack.Count > 0;
            var parentActivityId = hasParent ? parentActivityStack.Pop() : Guid.Empty;

            if (hasParent)
                Tracer.TraceTransfer(TransferBackId, FormatTransferBack(args), parentActivityId);

            Tracer.TraceEvent(TraceEventType.Stop, StopId, FormatStop(args));
            Trace.CorrelationManager.StopLogicalOperation();

            if (hasParent)
                Trace.CorrelationManager.ActivityId = parentActivityId;

            Tracer.Flush();
        }


        /// <summary>
        /// Returns <paramref name="value"/> as <see cref="IEnumerable"/>, if it is an collection, and not an string.
        /// Otherwise return <see langword="null"/>.
        /// </summary>
        public static IEnumerable AsAnCollection(object value) {
            if (value == null)
                return null;

            var type = value.GetType();

            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type)
                ? (IEnumerable) value
                : null;
        }
    }
}