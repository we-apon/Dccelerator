using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace Dccelerator.DataAccess.Ado.SqlClient {

    public abstract class SqlClientRepository : AdoNetRepository<SqlCommand, SqlParameter, SqlConnection> {
        #region Overrides of AdoNetRepository<SqlCommand,SqlParameter,SqlConnection>
        

        protected override SqlParameter ParameterWith(string name, Type type, object value) {
            return new SqlParameter('@' + name, type.SqlType()) { Value = value };
        }


        protected override SqlCommand CommandFor(string commandText, SqlConnection connection, IEnumerable<SqlParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var command = new SqlCommand(commandText, connection) {CommandType = type};
            foreach (var sqlParameter in parameters) {
                command.Parameters.Add(sqlParameter);
            }
            return command;
        }

        #endregion
    }
}