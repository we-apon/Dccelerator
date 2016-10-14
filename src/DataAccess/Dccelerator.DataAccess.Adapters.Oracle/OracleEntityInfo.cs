using System;
using Dccelerator.DataAccess.Ado.Implementation;
using Oracle.ManagedDataAccess.Client;
using Dccelerator.DataAccess.Ado;

namespace Dccelerator.DataAccess.Adapters.Oracle {

    public sealed class OracleEntityInfo<TRepository> : AdoEntityInfo<TRepository, OracleDbType> where TRepository : class, IAdoNetRepository {
        public OracleEntityInfo(Type entityType) : base(entityType) {}
        

        protected override IAdoEntityInfo GetInstanse(Type type) {
            return new OracleEntityInfo<TRepository>(type);
        }

        public override OracleDbType GetDefaultDbType(Type propertyType) {
            OracleDbType oracleType;

            if (propertyType.IsAssignableFrom(typeof(Guid)))
                oracleType = OracleDbType.Raw;
            else if (propertyType.IsAssignableFrom(typeof(string)))
                oracleType = OracleDbType.NVarchar2;
            else if (propertyType.IsAssignableFrom(typeof(DateTime)))
                oracleType = OracleDbType.Date;
            else if (propertyType.IsAssignableFrom(typeof(Boolean)))
                oracleType = OracleDbType.Byte;
            else if (propertyType.IsAssignableFrom(typeof(int)))
                oracleType = OracleDbType.Int32;
            else if (propertyType.IsAssignableFrom(typeof(long)))
                oracleType = OracleDbType.Int64;
            else if (propertyType.IsAssignableFrom(typeof(float)) || propertyType.IsAssignableFrom(typeof(double)))
                oracleType = OracleDbType.Double;
            else if (propertyType.IsAssignableFrom(typeof(decimal)))
                oracleType = OracleDbType.Decimal;
            else if (propertyType.IsAssignableFrom(typeof(byte[])))
                oracleType = OracleDbType.Blob;
            else
                oracleType = OracleDbType.NVarchar2;

            return oracleType;
        }
    }
}