using System;
using Dccelerator.DataAccess.Ado.ReadingRepositories;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    class SqlClientCommonReadingRepository : CachedReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            throw new NotImplementedException();
        }

        #endregion
    }
}