using System;
using Dccelerator.DataAccess.Ado.ReadingRepositories;


namespace Dccelerator.DataAccess.Ado.SqlClient {
    sealed class SqlClientDirectReadingRepository : DirectReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }

        #endregion
    }
}