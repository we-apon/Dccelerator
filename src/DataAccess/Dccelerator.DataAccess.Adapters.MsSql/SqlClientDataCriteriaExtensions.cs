using System.Data.SqlClient;
using JetBrains.Annotations;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    public static class SqlClientDataCriteriaExtensions {

        [NotNull]
        public static SqlParameter ToSqlParameter([NotNull] this IDataCriterion criterion) {
            return new SqlParameter(criterion.Name, criterion.Type.SqlType()) {Value = criterion.Value};
        }
    }
}