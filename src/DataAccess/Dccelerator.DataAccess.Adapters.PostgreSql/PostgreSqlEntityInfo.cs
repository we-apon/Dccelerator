using System;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.Implementation;
using NpgsqlTypes;


namespace Dccelerator.DataAccess.Adapters.PostgreSql {
    public class PostgreSqlEntityInfo<TRepository> : AdoEntityInfo<TRepository, NpgsqlDbType> where TRepository : class, IAdoNetRepository {
        public PostgreSqlEntityInfo(Type entityType) : base(entityType) { }
        protected override IAdoEntityInfo GetInstance(Type type) {
            throw new NotImplementedException();
        }


        public override NpgsqlDbType GetDefaultDbType(Type propertyType) {
            throw new NotImplementedException();
        }
    }
}