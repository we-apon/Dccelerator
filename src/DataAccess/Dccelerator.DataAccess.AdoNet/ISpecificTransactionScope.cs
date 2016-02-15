using System;


namespace Dccelerator.DataAccess.Ado {
    public interface ISpecificTransactionScope : IDisposable {
        void Complete();
    }
}