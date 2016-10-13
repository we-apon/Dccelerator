using System;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public class OracleDirectReadingRepository : DirectReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadLock();
    }

    public class OracleCommonReadingRepository : CachedReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadLock();
    }



}