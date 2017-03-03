
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using PostSharp.Aspects;

namespace Dccelerator.TraceSourceAttributes {

    /// <summary>
    /// <para xml:lang="en"></para>
    /// <para xml:lang="ru">
    /// Базовый класс, реализующий всю логику PostSharp-атрибута для трассировки выполнения методов. 
    /// Трассировка выполняется фреймворком System.Diagnostics.TraceSource, встронным в .Net Framework.
    /// Кстати, для знакомства с System.Diagnostics.TraceSource хорошо подходит документация расширяюшего его фрейворка - <see hrev="https://essentialdiagnostics.codeplex.com/">Essential.Diagnostics</see>.
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class TraceSourceAttributeBase : OnMethodBoundaryAspect {

        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Глобальный экземпляр трассировщика. 
        /// Используется, если при объявлении атрибута не было переопределено свойство <see cref="SourceName"/>.
        /// Для имени используется короткое имя сборки приложения (<see cref="Assembly.GetEntryAssembly">Assembly.GetEntryAssembly()</see>),
        /// либо имя исполняемой сборки (<see cref="Assembly.GetExecutingAssembly">Assembly.GetExecutingAssembly()</see>), что будет соответствовать сборке 'Dccelerator.TraceSourceAspects'.
        /// </para>
        /// </summary>
        /// <see cref="SourceName"/>
        /// <seealso cref="Tracer"/>
        static readonly TraceSource _globalTrace = new TraceSource((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name);


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Используемый атрибутом экземпляр трассировщика. 
        /// Если было проинициализировно свойство <see cref="SourceName"/> - его значение используется в качестве имени <see cref="TraceSource"/>.
        /// Если нет - используется короткое имя сборки приложения (<see cref="Assembly.GetEntryAssembly">Assembly.GetEntryAssembly()</see>),
        /// либо имя исполняемой сборки (<see cref="Assembly.GetExecutingAssembly">Assembly.GetExecutingAssembly()</see>), что в большинстве случаев будет соответствовать сборке 'Dccelerator.TraceSourceAspects'.
        /// Есть мнение, что лучше явно указывать <see cref="SourceName"/>, т.к. при выполнении тех же тестов <see cref="Assembly.GetEntryAssembly">Assembly.GetEntryAssembly()</see> ничего не вернёт.
        /// </para>
        /// </summary>
        /// <see cref="SourceName"/>
        protected virtual TraceSource Tracer => _trace ?? (_trace = string.IsNullOrWhiteSpace(SourceName) ? _globalTrace : new TraceSource(SourceName));

        [NonSerialized]
        TraceSource _trace;


        protected int? IdentifiedParameterIndex { get; set; }

        protected PropertyInfo IdentifierParameterProperty { get; set; }

        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Стек вызовов логируемых методов текущего потока. Содержит идентификаторы активностей сообщений трассировки.
        /// Стек начинает заполняться, когда свойство <see cref="Trace.CorrelationManager.ActivityId">Trace.CorrelationManager.ActivityId</see> не проинициализировано.
        /// Используется для передачи идентификатора предыдущей активности в метод <see cref="TraceSource.TraceTransfer"/>, при переходе обратно к методу, вызвавшему текущий (вверх по иерархии вызовов).
        /// </para>
        /// </summary>
        static readonly ThreadLocal<Stack<Guid>> _parentActicities = new ThreadLocal<Stack<Guid>>(() => new Stack<Guid>());


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Список параметров текущего метода. Инициализируется при <see cref="CompileTimeInitialize">компиляции</see>.
        /// Используется для форматирования аргументов методов.
        /// </para>
        /// </summary>
        protected virtual ParameterInfo[] Parameters { get; set; }


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Позвляет переопределить имя используемого <see cref="Tracer">трассировщика</see>.
        /// Если указано - используется оно.
        /// Если нет - используется короткое имя сборки приложения (<see cref="Assembly.GetEntryAssembly">Assembly.GetEntryAssembly()</see>),
        /// либо имя исполняемой сборки (<see cref="Assembly.GetExecutingAssembly">Assembly.GetExecutingAssembly()</see>), что будет соответствовать сборке 'Dccelerator.TraceSourceAspects'.
        /// Есть мнение, что лучше явно указывать <see cref="SourceName"/>, т.к. при выполнении тех же тестов <see cref="Assembly.GetEntryAssembly">Assembly.GetEntryAssembly()</see> ничего не вернёт.
        /// </para>
        /// </summary>
        /// <seealso cref="Tracer"/>
        public virtual string SourceName { get; set; }


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Имя текущего метода. Инициализируется при <see cref="CompileTimeInitialize">компиляции</see>.
        /// По умолчанию используется формат "Класс.Метод".
        /// Формат можно переопределить в методе <see cref="FormatCompileTimeMethodName"/>.
        /// </para>
        /// </summary>
        public virtual string MethodName { get; set; }


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Имя логической операции. Инициализируется при <see cref="CompileTimeInitialize">компиляции</see>.
        /// По умолчанию используется формат "Класс.Метод".
        /// Формат можно переопределить в методе <see cref="FormatCompileTimeLogicalOperation"/>.
        /// </para>
        /// </summary>
        public virtual string LogicalOperation { get; set; }


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Определяет, следует ли логировать каждый элемент аргументов-коллекций.
        /// По умолчанию - нет, не следует (<see langword="false"/>).
        /// </para>
        /// </summary>
        public virtual bool LogCollectionItems { get; set; }




        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Код события перехода потока выполненя из текущего метода в другой (т.е. код сообщения о событии вызова какого-либо метода, из текущего).
        /// <see hrev="https://essentialdiagnostics.codeplex.com/wikipage?title=Event%20Ids&referringTitle=Documentation">Статья</see> фреймворка Essential.Diagnostics проясняет, накой оно сдалось.
        /// </para>
        /// </summary>
        public virtual int TransferToId { get; set; } = 6000;
        

        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Код события возвращения потока выполненя из текущего метода в тот, что его вызвал.
        /// <see hrev="https://essentialdiagnostics.codeplex.com/wikipage?title=Event%20Ids&referringTitle=Documentation">Статья</see> фреймворка Essential.Diagnostics проясняет, накой оно сдалось.
        /// </para>
        /// </summary>
        public virtual int TransferBackId { get; set; } = 6010;


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Код события начала выполнения текущего метода.
        /// <see hrev="https://essentialdiagnostics.codeplex.com/wikipage?title=Event%20Ids&referringTitle=Documentation">Статья</see> фреймворка Essential.Diagnostics проясняет, накой оно сдалось.
        /// </para>
        /// </summary>
        public virtual int StartId { get; set; } = 1000;


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Код события окончания выполнения текущего метода.
        /// <see hrev="https://essentialdiagnostics.codeplex.com/wikipage?title=Event%20Ids&referringTitle=Documentation">Статья</see> фреймворка Essential.Diagnostics проясняет, накой оно сдалось.
        /// </para>
        /// </summary>
        public virtual int StopId { get; set; } = 8000;


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Код события исключения (эксепшона).
        /// <see hrev="https://essentialdiagnostics.codeplex.com/wikipage?title=Event%20Ids&referringTitle=Documentation">Статья</see> фреймворка Essential.Diagnostics проясняет, накой оно сдалось.
        /// </para>
        /// </summary>
        public virtual int ExceptionId { get; set; } = 9900;


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">Уровень события исключения (эксепшона). По умолчанию <see cref="TraceEventType.Critical"/>.</para>
        /// </summary>
        public virtual TraceEventType ExceptionsLevel { get; set; } = TraceEventType.Critical;



        protected virtual int? GetIdentifiedParameterIndex(ParameterInfo[] parameters, out PropertyInfo identifierProperty) {
            identifierProperty = null;

            if (parameters == null || parameters.Length == 0)
                return null;

            for (int i = 0; i < parameters.Length; i++) {
                identifierProperty = GetIdentifierPropertyOfType(parameters[i].ParameterType);
                if (identifierProperty != null)
                    return i;
            }

            return null;
        }


        protected virtual PropertyInfo GetIdentifierPropertyOfType(Type type) {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(x => x.CanRead && x.Name == "Id" && x.PropertyType == typeof(Guid)); 
        }


        protected virtual Guid GetActivityIdFromIdentifiedArgument(object value) {
            if (value == null)
                return Guid.NewGuid();

            try {
                return (Guid) IdentifierParameterProperty.GetValue(value, null);
            }
            catch (Exception e) {
                Tracer.TraceEvent(TraceEventType.Warning, 4000, $"Exception while getting identifier of object {value.GetType()} from property {IdentifierParameterProperty}\n{e}");
                return Guid.NewGuid();
            }
        }


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Возвращает идентификатор для новой активности логировщика
        /// </para>
        /// </summary>
        protected virtual Guid GetNewActivityGuid(MethodExecutionArgs args) {
            if (Parameters.Length == 0 || IdentifiedParameterIndex == null || args.Arguments.Count < IdentifiedParameterIndex.Value)
                return Guid.NewGuid();

            var argument = args.Arguments[IdentifiedParameterIndex.Value];
            var id = GetActivityIdFromIdentifiedArgument(argument);

            if (Trace.CorrelationManager.ActivityId == Guid.Empty || Trace.CorrelationManager.ActivityId != id)
                return id;

            return Guid.Empty;
        }

        protected virtual string FormatTransferTo(MethodExecutionArgs args) =>  $"Transfered to {MethodName}";
        protected virtual string FormatTransferBack(MethodExecutionArgs args) => "Transfered back";

        protected virtual string FormatStart(MethodExecutionArgs args) => $"{MethodName}{FormatInputArguments(args)}";
        protected virtual string FormatStop(MethodExecutionArgs args) =>  $"{MethodName}{FormatReturnValue(args.ReturnValue)}";

        protected virtual string FormatException(MethodExecutionArgs args) => args.Exception.ToString();
        protected virtual string FormatLogicalOperation(MethodExecutionArgs args) => LogicalOperation;


        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Возвращает отформатированное имя текущего метода. Вызывается при <see cref="CompileTimeInitialize">компиляции</see>.
        /// По умолчанию используется формат "Класс.Метод".
        /// </para>
        /// </summary>
        /// <seealso cref="MethodName"/>
        /// <seealso cref="CompileTimeInitialize"/>
        protected virtual string FormatCompileTimeMethodName(MethodBase method, AspectInfo aspectInfo) {
            return $"{(method.ReflectedType ?? method.DeclaringType)?.Name}.{method.Name}";
        }

        /// <summary>
        /// <para xml:lang="en"></para>
        /// <para xml:lang="ru">
        /// Возвращает отформатированное имя логической операции. Вызывается при <see cref="CompileTimeInitialize">компиляции</see>.
        /// По умолчанию используется формат "Класс.Метод".
        /// </para>
        /// </summary>
        /// <seealso cref="LogicalOperation"/>
        /// <seealso cref="CompileTimeInitialize"/>
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
            LogicalOperation = FormatCompileTimeLogicalOperation(method, aspectInfo);

            Parameters = method.GetParameters();

            PropertyInfo identifierProperty;
            IdentifiedParameterIndex = GetIdentifiedParameterIndex(Parameters, out identifierProperty);
            IdentifierParameterProperty = identifierProperty;
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

            Trace.CorrelationManager.ActivityId = hasParent ? parentActivityId : Guid.Empty;
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