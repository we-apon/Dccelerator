using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dccelerator.UnFastReflection;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    public static class SqlClientParameterExtensions {
        static readonly ConcurrentDictionary<Type, SqlDbType> _sqlDbTypes = new ConcurrentDictionary<Type, SqlDbType>();

        /// <summary>
        /// <para>Returns <see cref="SqlDbType"/> most likelly corresponding to passed <paramref name="type"/></para>
        /// <para>Mappings are following:<br/>
        /// An <see langword="enum"/>: <see cref="SqlDbType.Int"/> <br/>
        /// <see cref="Guid"/> and <see cref="Nullable{Guid}"/>: <see cref="SqlDbType.UniqueIdentifier"/><br/>
        /// <see cref="string"/>: <see cref="SqlDbType.NVarChar"/><br/>
        /// <see cref="DateTime"/> and <see cref="Nullable{DateTime}"/>: <see cref="SqlDbType.DateTime"/><br/>
        /// ...todo: more comments
        /// </para>
        /// </summary>
        public static SqlDbType SqlType([NotNull] this Type type) {
            SqlDbType sqlType;
            if (_sqlDbTypes.TryGetValue(type, out sqlType))
                return sqlType;

            if (type.IsAssignableFrom(_guidType))
                sqlType = SqlDbType.UniqueIdentifier;
            else if (type.IsAssignableFrom(_stringType))
                sqlType = SqlDbType.NVarChar;
#if NETSTANDARD1_3
            else if (type.GetInfo().IsEnum)
                sqlType = SqlDbType.Int;
#else
            else if (type.IsEnum)
                sqlType = SqlDbType.Int;
#endif
            else if (type.IsAssignableFrom(_dateTimeType))
                sqlType = SqlDbType.DateTime;
            else if (type.IsAssignableFrom(_booleanType))
                sqlType = SqlDbType.Bit;
            else if (type.IsAssignableFrom(_byteType))
                sqlType = SqlDbType.TinyInt;
            else if (type.IsAssignableFrom(_shortType))
                sqlType = SqlDbType.SmallInt;
            else if (type.IsAssignableFrom(_intType))
                sqlType = SqlDbType.Int;
            else if (type.IsAssignableFrom(_longType))
                sqlType = SqlDbType.BigInt;
            else if (type.IsAssignableFrom(_floatType))
                sqlType = SqlDbType.Real;
            else if (type.IsAssignableFrom(_doubleType))
                sqlType = SqlDbType.Float;
            else if (type.IsAssignableFrom(_decimalType))
                sqlType = SqlDbType.Decimal;
            else if (type.IsAssignableFrom(_byteArrayType))
                sqlType = SqlDbType.VarBinary;
            else if (type.IsAssignableFrom(_dataTableType))
                sqlType = SqlDbType.Structured;
            else
                sqlType = SqlDbType.Text;

            _sqlDbTypes[type] = sqlType;
            return sqlType;
        }


        /// <summary>
        /// Генерирует массив <see cref="SqlParameter"/>, пользуясь рефлексией.
        /// Параметры создаются для всех переданных пропертей, доступных на чтение.
        /// </summary>
        /// <param name="entity">Объект, пропертя которого отражают колонки таблиц БД, или параметры хранимой процедуры</param>
        /// <param name="includedProperties">Пропертя, включаемые в генерацию</param>
        /// <exception cref="InvalidOperationException">Параметр includedProperties должен содержать хотя бы одно значение</exception>
        [NotNull]
        public static SqlParameter[] ToSelectedSqlParameters<T>([NotNull] this T entity, [NotNull] params Expression<Func<T, object>>[] includedProperties) where T : class {
            if (!includedProperties.Any())
                throw new InvalidOperationException("Параметр includedProperties должен содержать хотя бы одно значение");

            var includedNames = includedProperties.MemberExpressions().Select(x => x.Member.Name);
            var props = entity.GetType().GetProperties().Where(x => x.CanRead && includedNames.Contains(x.Name)).ToArray();
            return GetParametersFor(props, entity);
        }




        /// <summary>
        /// Генерирует массив <see cref="SqlParameter"/>, пользуясь рефлексией.
        /// Параметры создаются для всех доступных для чтения пропертей с типами struct, <see cref="string"/>, <see cref="Guid"/> или <see cref="Nullable"/>.
        /// При желании, можно указать пропертя, которые следует исключить из обработки.
        /// </summary>
        /// <param name="entity">Объект, пропертя которого отражают колонки таблиц БД, или параметры хранимой процедуры</param>
        /// <param name="excludedProperties">Пропертя, исключаемые из генерации параметров</param>
        [NotNull]
        public static SqlParameter[] ToSqlParameters<T>([NotNull] this T entity, [NotNull] params Expression<Func<T, object>>[] excludedProperties) where T : class {
            var excludedPropNames = excludedProperties.MemberExpressions().Select(x => x.Member.Name).ToArray();

            var props = entity.GetType().GetProperties()
                .Where(x => x.CanRead
                         && !excludedPropNames.Contains(x.Name)
                         && !x.IsDefined<NotPersistedAttribute>()
                         && (x.PropertyType.IsAssignableFrom(_stringType) || _byteArrayType.IsAssignableFrom(x.PropertyType)
                             || (!_enumerableType.IsAssignableFrom(x.PropertyType) && !x.PropertyType.GetInfo().IsClass)))
                .ToArray();

            return GetParametersFor(props, entity);
        }


#region private

        static readonly Type _stringType = typeof (string);
        static readonly Type _booleanType = typeof (bool);
        static readonly Type _dateTimeType = typeof (DateTime);
        static readonly Type _guidType = typeof (Guid);
        static readonly Type _byteType = typeof(byte);
        static readonly Type _shortType = typeof(short);
        static readonly Type _intType = typeof(int);
        static readonly Type _longType = typeof(long);
        static readonly Type _floatType = typeof (float);
        static readonly Type _doubleType = typeof (double);
        static readonly Type _decimalType = typeof (decimal);
        static readonly Type _byteArrayType = typeof (byte[]);
        static readonly Type _enumerableType = typeof (IEnumerable);
        static readonly Type _dataTableType = typeof(DataTable);

        const string Dog = "@";


        [NotNull]
        static SqlParameter[] GetParametersFor<T>([NotNull] IList<PropertyInfo> props, [NotNull] T entity) where T : class {
            var parameters = new SqlParameter[props.Count];
            for (var i = 0; i < props.Count; i++)
                parameters[i] = GetParameterFor(props[i], entity);

            return parameters;
        }


        [NotNull]
        static SqlParameter GetParameterFor<T>([NotNull] PropertyInfo prop, [NotNull] T entity) where T : class {
            var type = prop.PropertyType.SqlType();
            return new SqlParameter(Dog + prop.Name, type) {Value = prop.GetValue(entity, null)};
        }

#endregion

    }
}