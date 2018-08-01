using System;


namespace Dccelerator.DataAccess.Logging {
    public interface IAsyncLoggerManager {
        ILoggerSession BeginLogSession();
        ILoggerSession BeginLogSession(Guid sessionId);
        ILoggerSession BeginLogSession(byte[] sessionId);
        ILoggerSession BeginLogSession(string sessionId);
    }
}