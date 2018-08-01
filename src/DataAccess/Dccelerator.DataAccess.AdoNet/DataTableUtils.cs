#if !NETSTANDARD1_3

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dccelerator.DataAccess.Implementation;
using Dccelerator.UnFastReflection;


namespace Dccelerator.DataAccess.Ado {
    public static class DataTableUtils {

        /// <summary>
        /// Creates <see cref="DataTable"/> from every item of <paramref name="collection"/> using properties and column order from <paramref name="expressions"/> parameter.
        /// </summary>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, params Expression<Func<T, object>>[] expressions) where T : class
        {
            if (expressions.Any()) {
                var expFields = expressions.MemberExpressions().Select(x => x.Member.Name).ToArray();
                return ToDataTable(collection, expFields);
            }
            var columns = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.IsPersisted())
                .Select(x => x.Name)
                .ToArray();

            return ToDataTable(collection, columns);
        }




        static readonly ConcurrentDictionary<string, DataTable> _structureMapping = new ConcurrentDictionary<string, DataTable>();


        /// <summary>
        /// Creates <see cref="DataTable"/> from every item of <paramref name="collection"/> using properties and column order from <paramref name="columnsSequence"/> parameter.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="collection">Collection of an entities</param>
        /// <param name="columnsSequence">Sequence of property names of <typeparamref name="T"/></param>
        /// <param name="tableName"></param>
        /// <returns><see cref="DataTable"/> filled with data from <paramref name="collection"/></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, string[] columnsSequence, string tableName = null) where T: class {
            var type = typeof(T);

            var key = string.Concat(type.FullName, ";", tableName, ";", columnsSequence.Length);

            if (!_structureMapping.TryGetValue(key, out var tableDefinition)) {

                if (string.IsNullOrWhiteSpace(tableName)) {
                    tableName = type.Name;
                    var lastChar = tableName.Last();
                    tableName += char.ToLowerInvariant(lastChar).Equals('s') ? "es" : "s";
                }

                var props = columnsSequence.Select(x => type.GetProperty(x, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).ToList();

                tableDefinition = new DataTable(tableName);
                props.ForEach(x => tableDefinition.Columns.Add(new DataColumn(x.Name, Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType)));

                _structureMapping.TryAdd(key, tableDefinition);
            }


            var table = tableDefinition.Clone();
            
            foreach (var entity in collection) {
                var row = table.NewRow();

                foreach (var name in columnsSequence) {
                    if (!entity.TryGet(name, out object value)) {
                        continue;
                    }

                    row[name] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;

        }


        /// <summary>
        /// Creates <see cref="DataTable"/> from every item of <paramref name="collection"/> using specified table <paramref name="structure"/>.
        /// </summary>
        /// <typeparam name="T">Any type.</typeparam>
        /// <param name="collection">Collection of an entities.</param>
        /// <param name="structure"><see cref="DataTable"/> structure definition.</param>
        /// <returns><see cref="DataTable"/> filled with data from <paramref name="collection"/>.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, DataTable structure) where T : class
        {

            var table = structure.Clone();

            foreach (var entity in collection) {
                var row = table.NewRow();

                foreach (DataColumn column in table.Columns) {
                    var name = column.ColumnName;
                    if (!entity.TryGet(name, out object value)) {
                        continue;
                    }

                    row[name] = value ?? column.DefaultValue ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;

        }



        /// <summary>
        /// Creates <see cref="DataTable"/> containing one single row with <paramref name="entity"/> data, using properties and column order from <paramref name="expressions"/> parameter.
        /// </summary>
        public static DataTable ToSingleDataTable<T>(this T entity, params Expression<Func<T, object>>[] expressions) where T : class
        {
            return ToDataTable(new[] {entity}, expressions);
        }




        /// <summary>
        /// Converts <see cref="DataTable"/> to collection.
        /// <para>Evety row of <see cref="DataTable"/> converted to <typeparamref name="T"/> item and will be returned as yilded <see cref="IEnumerable{T}"/>.</para>
        /// </summary>
        public static IEnumerable<T> AsCollectionOf<T>(this DataTable table) where T : class, new() {
            return table.Rows.Cast<DataRow>().Select(x => x.To<T>());
        }
    }
}

#endif