using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dccelerator.DataAccess.Ado;
using Npgsql;


namespace Dccelerator.DataAccess.Adapters.PostgreSql {
    /// <summary>
    /// SQL Client Repository for PostgreSQL database
    /// </summary>
    public abstract class PostgreSqlClientRepository : AdoNetRepository<NpgsqlCommand, NpgsqlParameter, NpgsqlConnection> {

        protected override string DatabaseSpecificNameOf(string parameter) => "@" + parameter;


        protected override NpgsqlParameter ParameterWith(IEntityInfo info, IDataCriterion criterion) {
            var sqlInfo = (IAdoEntityInfo<SqlDbType>) info;
            return new NpgsqlParameter(DatabaseSpecificNameOf(criterion), sqlInfo.GetParameterDbType(criterion)) {Value = criterion.Value};
        }


        protected override NpgsqlCommand CommandFor(string commandText, DbActionArgs args, IEnumerable<NpgsqlParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var connection = args.Connection as NpgsqlConnection;
            var transaction = args.Transaction as NpgsqlTransaction;

            if (connection == null) {
                if (args.Connection == null)
                    throw new InvalidOperationException($"{nameof(args.Connection)} is not instantiated!");

                throw new InvalidOperationException($"{nameof(args.Connection)} has unexpected type {args.Connection.GetType()}!");
            }

            var command = transaction == null
                ? new NpgsqlCommand(commandText, connection) {CommandType = type}
                : new NpgsqlCommand(commandText, connection, transaction) {CommandType = type};

            foreach (var sqlParameter in parameters) {
                command.Parameters.Add(sqlParameter);
            }

            return command;
        }
    }
}