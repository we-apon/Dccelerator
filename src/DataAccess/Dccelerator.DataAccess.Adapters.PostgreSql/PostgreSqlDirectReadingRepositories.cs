using System;
using System.Data.SqlClient;
using Dccelerator.DataAccess.Ado.Implementation;
using Npgsql;


namespace Dccelerator.DataAccess.Adapters.PostgreSql {

    static class DeadlockPostgreSqlExtensionExtension {
        internal static bool IsDeadlock(this Exception exception) => (exception as NpgsqlException)?.ErrorCode == byte.Parse("40P01"); //? 40P01 is Postgres deadlock detected error
    }


    public class PostgreSqlDirectReadingRepository : DirectReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadlock();
    }


    public class PostgreSqlForcedCacheReadingRepository : ForcedCacheReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadlock();
    }
}