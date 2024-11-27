namespace MyORM.CLI.Messaging.Interfaces;

/// <summary>
/// Interface for logging messages.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
    void LogError(string messageKey);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
    /// <param name="args">Additional message arguments</param>
    void LogError(string messageKey, string[]? args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="exception">Exception to log</param>
	void LogError(Exception exception);

    /// <summary>
    /// Logs a success message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
	void LogSuccess(string messageKey);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
	void LogWarning(string messageKey);

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
    /// <param name="args">Additional message arguments</param>
	void LogInfo(string messageKey, string[]? args);

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="customMessage">Custom message to log</param>
	void LogInfo(string customMessage);
}

