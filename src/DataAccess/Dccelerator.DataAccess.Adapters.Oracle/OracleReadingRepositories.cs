using System;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Adapters.Oracle {
    public sealed class OracleDirectReadingRepository : DirectReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadLock();
    }
    
    public sealed class OracleForcedCacheReadingRepository : ForcedCacheReadingRepository {
        protected override bool IsDeadlock(Exception exception) => exception.IsDeadLock();
    }



}