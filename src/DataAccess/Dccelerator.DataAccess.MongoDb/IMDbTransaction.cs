namespace Dccelerator.DataAccess.MongoDb {
    public interface IMDbTransaction {
        void Begin();

        bool Commit();

        bool Abort();
    }
}