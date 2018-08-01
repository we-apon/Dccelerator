namespace Dccelerator.DataAccess {
    public interface IIdentified<TKey> {
        TKey Id { get; set; }
    }
}