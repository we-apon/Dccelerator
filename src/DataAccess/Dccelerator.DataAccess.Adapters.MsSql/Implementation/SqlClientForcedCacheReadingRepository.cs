using System;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Ado.SqlClient.Implementation {
    sealed class SqlClientForcedCacheReadingRepository : ForcedCacheReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }

        #endregion
    }
}