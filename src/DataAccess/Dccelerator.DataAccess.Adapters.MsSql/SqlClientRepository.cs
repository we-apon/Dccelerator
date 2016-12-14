using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    public abstract class SqlClientRepository : AdoNetRepository<SqlCommand, SqlParameter, SqlConnection> {

        protected override string DatabaseSpecificNameOf(string parameter) => "@" + parameter;



        protected override SqlParameter ParameterWith(IEntityInfo info, IDataCriterion criterion) {
            var sqlInfo = (IAdoEntityInfo<SqlDbType>) info;
            return new SqlParameter(DatabaseSpecificNameOf(criterion), sqlInfo.GetParameterDbType(criterion)) {Value = criterion.Value};
        }




        protected override SqlCommand CommandFor(string commandText, DbActionArgs args, IEnumerable<SqlParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var connection = args.Connection as SqlConnection;
            var transaction = args.Transaction as SqlTransaction;

            if (connection == null) {
                if (args.Connection == null)
                    throw new InvalidOperationException($"{nameof(args.Connection)} is not instantiated!");

                throw new InvalidOperationException($"{nameof(args.Connection)} has unexpected type {args.Connection.GetType()}!");
            }

            var command = transaction == null 
                ? new SqlCommand(commandText, connection) { CommandType = type}
                : new SqlCommand(commandText, connection, transaction) {CommandType = type};

            foreach (var sqlParameter in parameters) {
                command.Parameters.Add(sqlParameter);
            }

            return command;
        }

    }
}