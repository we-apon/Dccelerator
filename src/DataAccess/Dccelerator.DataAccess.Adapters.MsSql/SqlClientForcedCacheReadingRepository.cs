using System;
using Dccelerator.DataAccess.Ado.ReadingRepositories;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    sealed class SqlClientForcedCacheReadingRepository : ForcedCacheReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            throw new NotImplementedException();
        }

        #endregion
    }
}