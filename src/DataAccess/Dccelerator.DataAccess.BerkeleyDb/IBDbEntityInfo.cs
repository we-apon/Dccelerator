

namespace Dccelerator.DataAccess.BerkeleyDb {
    public interface IBDbEntityInfo : IEntityInfo {
        IBDbRepository Repository { get; }
    }
}