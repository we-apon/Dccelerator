namespace Dccelerator.DataAccess {

    /// <summary>
    /// Specifies the transaction locking behavior for the connection.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    /// <remarks>
    /// Copied from Microsoft's System.Data namespace
    /// </remarks>
    public enum IsolationLevel
    {
        Unspecified = -1,
        Chaos = 16,
        ReadUncommitted = 256,
        ReadCommitted = 4096,
        RepeatableRead = 65536,
        Serializable = 1048576,
        Snapshot = 16777216,
    }
}