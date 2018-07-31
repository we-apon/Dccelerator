#if !NETSTANDARD1_3

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado.SqlClient
{
    public static class SqlClientDataTableExtensions {

        /// <summary>
        /// Вписывает таблицу в базу данных. Может кидать эксепшоны
        /// </summary>
        /// <exception cref="ArgumentException">Передается значение null или пустая строка (""), а таблица принадлежит коллекции. </exception>
        /// <exception cref="DuplicateNameException">Таблица принадлежит коллекции, которая уже содержит таблицу с таким же именем. (При сравнении учитывается регистр).</exception>
        /// <seealso cref="DataTableUtils.ToDataTable{T}(IEnumerable{T}, string[], string)"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] DbActionArgs args, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeout = 1200) {
            using (var bulk = new SqlBulkCopy((SqlConnection)args.Connection, options, (SqlTransaction)args.Transaction) {
                DestinationTableName = table.TableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeout
            }) {
                bulk.WriteToServer(table);
            }
        }


        /// <summary>
        /// Вписывает таблицу в базу данных. Может кидать эксепшоны
        /// </summary>
        /// <exception cref="ArgumentException">Передается значение null или пустая строка (""), а таблица принадлежит коллекции. </exception>
        /// <exception cref="DuplicateNameException">Таблица принадлежит коллекции, которая уже содержит таблицу с таким же именем. (При сравнении учитывается регистр).</exception>
        /// <seealso cref="DataTableUtils.ToDataTable{T}(IEnumerable{T}, string[], string)"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] string connection, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeout = 1200) {
            using (var bulk = new SqlBulkCopy(connection, options) {
                DestinationTableName = table.TableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeout
            }) {
                bulk.WriteToServer(table);
            }
        }


        /// <summary>
        /// Вписывает таблицу в базу данных. Может кидать эксепшоны
        /// </summary>
        /// <exception cref="ArgumentException">Передается значение null или пустая строка (""), а таблица принадлежит коллекции. </exception>
        /// <exception cref="DuplicateNameException">Таблица принадлежит коллекции, которая уже содержит таблицу с таким же именем. (При сравнении учитывается регистр).</exception>
        /// <seealso cref="DataTableUtils.ToDataTable{T}(IEnumerable{T}, string[], string)"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] SqlConnection connection, [NotNull] SqlTransaction transaction, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeout = 1200) {
            using (var bulk = new SqlBulkCopy(connection, options, transaction) {
                DestinationTableName = table.TableName,
                BatchSize = batchSize,
                BulkCopyTimeout = timeout
            }) {
                bulk.WriteToServer(table);
            }
        }

    }
}

#endif
