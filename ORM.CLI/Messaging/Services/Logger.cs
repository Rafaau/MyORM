using MyORM.CLI.Messaging.Interfaces;

namespace MyORM.CLI.Messaging.Services;

/// <summary>
/// Logger class for logging messages.
/// </summary>
public class Logger : ILogger
{
    /// <summary>
    /// Error message instance.
    /// </summary>
    private readonly ErrorMessage _errorMessage = new();

    /// <summary>
    /// Success message instance.
    /// </summary>
    private readonly SuccessMessage _successMessage = new();

    /// <summary>
    /// Warning message instance.
    /// </summary>
    private readonly WarningMessage _warningMessage = new();

    /// <summary>
    /// Info message instance.
    /// </summary>
	private readonly InfoMessage _infoMessage = new();

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
    /// <param name="args">Additional message arguments</param>
	public void LogError(string messageKey, string[]? args) => _errorMessage.InvokeMessage(messageKey, args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
	public void LogError(string customMessage) => _errorMessage.InvokeCustomMessage(customMessage);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="exception">Exception to log</param>
	public void LogError(Exception exception) => _errorMessage.InvokeErrorMessage(exception);

    /// <summary>
    /// Logs a success message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
	public void LogSuccess(string messageKey) => _successMessage.InvokeMessage(messageKey);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
	public void LogWarning(string messageKey) => _warningMessage.InvokeMessage(messageKey);

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="messageKey">Message key</param>
    /// <param name="args">Additional message arguments</param>
	public void LogInfo(string messageKey, string[]? args) => _infoMessage.InvokeMessage(messageKey, args);

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="customMessage">Custom message to log</param>
	public void LogInfo(string customMessage) => _infoMessage.InvokeCustomMessage(customMessage);
}

