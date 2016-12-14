using System;
using System.Collections.Generic;
using System.Data;
using Dccelerator.DataAccess.Ado;
using Oracle.ManagedDataAccess.Client;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public abstract class OracleRepository : AdoNetRepository<OracleCommand, OracleParameter, OracleConnection> {

        protected override string DatabaseSpecificNameOf(string parameter) => ":" + parameter;
        
        protected override OracleParameter ParameterWith(IEntityInfo info, IDataCriterion criterion) {
            var oraInfo = (IAdoEntityInfo<OracleDbType>) info;
            return new OracleParameter(DatabaseSpecificNameOf(criterion), oraInfo.GetParameterDbType(criterion)) {Value = criterion.Value};
        }
        
        protected override OracleCommand CommandFor(string commandText, DbActionArgs args, IEnumerable<OracleParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var connection = args.Connection as OracleConnection;
            var transaction = args.Transaction as OracleTransaction;


            if (connection == null) {
                if (args.Connection == null)
                    throw new InvalidOperationException($"{nameof(args.Connection)} is not instantiated!");

                throw new InvalidOperationException($"{nameof(args.Connection)} has unexpected type {args.Connection.GetType()}!");
            }

            var command = transaction == null
                ? new OracleCommand(commandText, connection) {CommandType = type} 
                : new OracleCommand(commandText, connection) { Transaction = transaction, CommandType = type};

            foreach (var oracleParameter in parameters) {
                command.Parameters.Add(oracleParameter);
            }

            return command;
        }
        
    }
}