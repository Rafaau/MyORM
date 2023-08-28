namespace CLI.Messaging;

internal class WarningMessage : Message
{
    public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Yellow;

    public override Dictionary<string, string> MessageContent { get; } = new()
    {
    };
}

