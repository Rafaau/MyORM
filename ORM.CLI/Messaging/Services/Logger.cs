using MyORM.CLI.Messaging.Interfaces;

namespace MyORM.CLI.Messaging.Services;

public class Logger : ILogger
{
	private readonly ErrorMessage _errorMessage = new();
	private readonly SuccessMessage _successMessage = new();
	private readonly WarningMessage _warningMessage = new();
	private readonly InfoMessage _infoMessage = new();

	public void LogError(string messageKey, string[]? args) => _errorMessage.InvokeMessage(messageKey, args);
	public void LogError(string customMessage) => _errorMessage.InvokeCustomMessage(customMessage);
	public void LogError(Exception exception) => _errorMessage.InvokeErrorMessage(exception);
	public void LogSuccess(string messageKey) => _successMessage.InvokeMessage(messageKey);
	public void LogWarning(string messageKey) => _warningMessage.InvokeMessage(messageKey);
	public void LogInfo(string messageKey, string[]? args) => _infoMessage.InvokeMessage(messageKey, args);
	public void LogInfo(string customMessage) => _infoMessage.InvokeCustomMessage(customMessage);
}

