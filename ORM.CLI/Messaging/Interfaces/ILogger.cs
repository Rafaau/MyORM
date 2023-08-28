namespace CLI.Messaging;

public interface ILogger
{
    void LogError(string messageKey);
    void LogError(string messageKey, string[]? args);
    void LogSuccess(string messageKey);
    void LogWarning(string messageKey);
    void LogInfo(string messageKey, string[]? args);
}

