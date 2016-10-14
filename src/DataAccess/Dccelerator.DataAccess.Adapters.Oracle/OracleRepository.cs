using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dccelerator.DataAccess.Ado;
using Oracle.ManagedDataAccess.Client;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public abstract class OracleRepository : AdoNetRepository<OracleCommand, OracleParameter, OracleConnection> {
        protected override OracleParameter ParameterWith(IEntityInfo info, IDataCriterion criterion) {
            var oraInfo = (IAdoEntityInfo<OracleDbType>) info;
            return new OracleParameter(':' + criterion.Name, oraInfo.GetParameterDbType(criterion.Name)) {Value = criterion.Value};
        }


        protected override OracleCommand CommandFor(string commandText, OracleConnection connection,
            IEnumerable<OracleParameter> parameters, CommandType type = CommandType.StoredProcedure) {
            var command = new OracleCommand(commandText, connection) {CommandType = type};

            foreach (var oracleParameter in parameters) {
                command.Parameters.Add(oracleParameter);
            }

            return command;
        }

        protected override string ReadCommandText(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            return info.UsingQueries
                ? SelectQuery(info, criteria)
                : SelectProcedure(info, criteria);
        }

        protected override string InsertCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return info.UsingQueries
                ? InsertQuery(info, entity)
                : InsertProcedure(info, entity);
        }



        protected override string UpdateCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return info.UsingQueries
                ? UpdateQuery(info, entity)
                : UpdateProcedure(info, entity);
        }



        protected override string DeleteCommandText<TEntity>(IEntityInfo info, TEntity entity) {
            return info.UsingQueries
                ? DeleteQuery(info, entity)
                : DeleteProcedure(info, entity);
        }




        protected internal virtual string SelectQuery(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            var builder = new StringBuilder("select * from ").Append(info.EntityName).Append(" where ");
            foreach (var criterion in criteria) {
                builder.Append(criterion.Name).Append(" = :").Append(criterion.Name).Append(" and ");
            }
            return builder.Remove(builder.Length-5, 5).ToString();
        }


        protected internal virtual string InsertQuery<TEntity>(IEntityInfo info, TEntity entity) {
            var builder = new StringBuilder("insert into ").Append(info.EntityName).Append(" ( ");
            foreach (var property in info.PersistedProperties.Keys) {
                builder.Append(property).Append(", ");
            }

            builder.Remove(builder.Length - 2, 2).Append(" )\n values ( ");

            foreach (var property in info.PersistedProperties.Keys) {
                builder.Append(":").Append(property).Append(", ");
            }

            return builder.Remove(builder.Length-2, 1).Append(")").ToString();
        }


        protected internal virtual string UpdateQuery<TEntity>(IEntityInfo info, TEntity entity) {
            var builder = new StringBuilder("update ").Append(info.EntityName).Append(" set ");
            foreach (var property in info.PersistedProperties.Keys) {
                builder.Append(property).Append(" = :").Append(property).Append(",\n");
            }

            var key = PrimaryKeyParameterOf(info, entity);

            builder.Remove(builder.Length - 2, 2);
            builder.Append("\nwhere ").Append(key.ParameterName).Append(" = :").Append(key.ParameterName);
            return builder.ToString();
        }


        protected internal virtual string DeleteQuery<TEntity>(IEntityInfo info, TEntity entity) {
            var builder = new StringBuilder("delete ").Append(info.EntityName);

            var key = PrimaryKeyParameterOf(info, entity);

            builder.Append("\nwhere").Append(key.ParameterName).Append(" = :").Append(key.ParameterName);
            return builder.ToString();
        }




        protected virtual string SelectProcedure(IEntityInfo info, IEnumerable<IDataCriterion> criteria) {
            throw new NotImplementedException();
        }

        protected virtual string InsertProcedure<TEntity>(IEntityInfo info, TEntity entity) {
            throw new NotImplementedException();
        }



        protected virtual string UpdateProcedure<TEntity>(IEntityInfo info, TEntity entity) {
            throw new NotImplementedException();
        }



        protected virtual string DeleteProcedure<TEntity>(IEntityInfo info, TEntity entity) {
            throw new NotImplementedException();
        }


    }
}