using System;


namespace Dccelerator.DataAccess.Ado.Transactions {
    public interface ISpecificTransactionScope : IDisposable {
        void Complete();
    }
}