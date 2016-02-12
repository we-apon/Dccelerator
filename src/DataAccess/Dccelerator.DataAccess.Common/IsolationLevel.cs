namespace Dccelerator.DataAccess {

    /// <summary>
    /// Specifies the transaction locking behavior for the connection.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    /// <remarks>
    /// Copied from Microsoft's System.Transactions namespace
    /// </remarks>
    public enum IsolationLevel
    {
        Serializable,
        RepeatableRead,
        ReadCommitted,
        ReadUncommitted,
        Snapshot,
        Chaos,
        Unspecified,
    }
}