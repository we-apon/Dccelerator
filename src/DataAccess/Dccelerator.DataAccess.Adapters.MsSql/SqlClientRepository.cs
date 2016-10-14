using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    public abstract class SqlClientRepository : AdoNetRepository<SqlCommand, SqlParameter, SqlConnection> {

        protected override SqlParameter ParameterWith(IEntityInfo info, IDataCriterion criterion) {
            var sqlInfo = (IAdoEntityInfo<SqlDbType>) info;
            return new SqlParameter('@' + criterion.Name, sqlInfo.GetParameterDbType(criterion.Name)) {Value = criterion.Value};
        }


        protected override SqlCommand CommandFor(string commandText, SqlConnection connection, IEnumerable<SqlParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var command = new SqlCommand(commandText, connection) {CommandType = type};
            foreach (var sqlParameter in parameters) {
                command.Parameters.Add(sqlParameter);
            }
            return command;
        }



        protected override string ReadCommandText(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            return string.Concat("obj_", info.EntityName, "_get_by_criteria");
        }



        protected override string InsertCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return string.Concat("obj_", info.EntityName, "_insert");
        }



        protected override string UpdateCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return string.Concat("obj_", info.EntityName, "_update");
        }



        protected override string DeleteCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return string.Concat("obj_", info.EntityName, "_delete");
        }
    }
}