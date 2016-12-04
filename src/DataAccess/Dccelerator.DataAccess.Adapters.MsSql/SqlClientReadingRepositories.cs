using System;
using System.Data.SqlClient;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Ado.SqlClient {

    static class DeadlockSqlExtensionExtension {
        internal static bool IsDeadlock(this Exception exception) => (exception as SqlException)?.Number == 1205; //? 1205 is number of MS-Sql deadlock error
    }

    sealed class SqlClientDirectReadingRepository : DirectReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadlock();
    }

    sealed class SqlClientForcedCacheReadingRepository : ForcedCacheReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadlock();
    }
}