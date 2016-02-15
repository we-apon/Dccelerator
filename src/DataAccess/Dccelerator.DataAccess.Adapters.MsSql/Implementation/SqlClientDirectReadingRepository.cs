using System;
using Dccelerator.DataAccess.Ado.BasicImplementation;


namespace Dccelerator.DataAccess.Ado.SqlClient.Implementation {
    sealed class SqlClientDirectReadingRepository : DirectReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }

        #endregion
    }
}