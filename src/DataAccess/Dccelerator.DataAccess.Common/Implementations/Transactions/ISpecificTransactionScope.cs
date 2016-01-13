using System;


namespace Dccelerator.DataAccess.Implementations.Transactions {
    public interface ISpecificTransactionScope : IDisposable {
        void Complete();
    }
}