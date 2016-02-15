using System;
using System.Data.SqlClient;


namespace Dccelerator.DataAccess.Ado.SqlClient.Implementation {
    static class DeadlockSqlExtensionExtension {
        internal static bool IsDeadlock(this Exception exception) {
            return (exception as SqlException)?.Number == 1205; //? 1205 is number of MS-Sql deadlock error
        }
    }
}