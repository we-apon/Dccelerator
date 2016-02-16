using System;
using Dccelerator.DataAccess.Ado.Implementation;


namespace Dccelerator.DataAccess.Ado.SqlClient.Implementation {
    class SqlClientCommonReadingRepository : CachedReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }

        #endregion
    }
}