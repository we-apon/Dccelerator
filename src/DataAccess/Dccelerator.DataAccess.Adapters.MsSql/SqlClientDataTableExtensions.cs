using System;
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
        /// <seealso cref="DataTableUtils.ToDataTable{T}"/>
        public static void BulkInsert([NotNull] this DataTable table, [NotNull] string connection) {
            using (var bulk = new SqlBulkCopy(connection) {
                DestinationTableName = table.TableName,
                BatchSize = 5000,
                BulkCopyTimeout = 1200
            }) {
                bulk.WriteToServer(table);
            }
        }
    }
}