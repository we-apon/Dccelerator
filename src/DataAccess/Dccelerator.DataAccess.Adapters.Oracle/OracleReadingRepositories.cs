using System;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public sealed class OracleDirectReadingRepository : DirectReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadLock();
    }

    public sealed class OracleCommonReadingRepository : CachedReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadLock();
    }



}