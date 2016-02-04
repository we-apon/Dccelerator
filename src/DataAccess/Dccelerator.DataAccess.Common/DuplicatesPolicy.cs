namespace Dccelerator.DataAccess {
    /// <summary>
    /// Policy for duplicate data items in the database. Allows a key/data
    ///             pair to be inserted into the database even if the key already exists.
    /// 
    /// </summary>
    /// <remarks>
    /// Copied from Berkeley Db.
    /// </remarks>
    public enum DuplicatesPolicy : uint {
        NONE = 0,
        SORTED = 2,
        UNSORTED = 16,
    }
}