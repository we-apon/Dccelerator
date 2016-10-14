using System;
using System.Data.SqlClient;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Ado.SqlClient {

    static class DeadlockSqlExtensionExtension {
        internal static bool IsDeadlock(this Exception exception) {
            return (exception as SqlException)?.Number == 1205; //? 1205 is number of MS-Sql deadlock error
        }
    }

    sealed class SqlClientDirectReadingRepository : DirectReadingRepository {
        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }
    }

    sealed class SqlClientCommonReadingRepository : CachedReadingRepository {
        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }
    }


    sealed class SqlClientForcedCacheReadingRepository : ForcedCacheReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }

        #endregion
    }
}