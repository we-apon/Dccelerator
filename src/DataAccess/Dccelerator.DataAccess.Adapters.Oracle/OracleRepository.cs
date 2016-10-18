using System.Collections.Generic;
using System.Data;
using Dccelerator.DataAccess.Ado;
using Oracle.ManagedDataAccess.Client;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public abstract class OracleRepository : AdoNetRepository<OracleCommand, OracleParameter, OracleConnection> {

        protected override string DatabaseSpecificNameOf(string parameter) {
            return ":" + parameter;
        }
        
        protected override OracleParameter ParameterWith(IEntityInfo info, IDataCriterion criterion) {
            var oraInfo = (IAdoEntityInfo<OracleDbType>) info;
            return new OracleParameter(DatabaseSpecificNameOf(criterion.Name), oraInfo.GetParameterDbType(criterion.Name)) {Value = criterion.Value};
        }
        
        protected override OracleCommand CommandFor(string commandText, OracleConnection connection,
            IEnumerable<OracleParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var command = new OracleCommand(commandText, connection) {CommandType = type};

            foreach (var oracleParameter in parameters) {
                command.Parameters.Add(oracleParameter);
            }

            return command;
        }
        
    }
}