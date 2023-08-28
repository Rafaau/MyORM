namespace CLI.Messaging;

public class Logger : ILogger
{
    private readonly ErrorMessage _errorMessage = new();
    private readonly SuccessMessage _successMessage = new();
    private readonly WarningMessage _warningMessage = new();
    private readonly InfoMessage _infoMessage = new();

    public void LogError(string messageKey) => _errorMessage.InvokeMessage(messageKey);
    public void LogError(string messageKey, string[]? args) => _errorMessage.InvokeMessage(messageKey, args);
    public void LogSuccess(string messageKey) => _successMessage.InvokeMessage(messageKey);
    public void LogWarning(string messageKey) => _warningMessage.InvokeMessage(messageKey);
    public void LogInfo(string messageKey, string[]? args) => _infoMessage.InvokeMessage(messageKey, args);
}

