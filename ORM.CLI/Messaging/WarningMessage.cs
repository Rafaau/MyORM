namespace MyORM.CLI.Messaging;

/// <summary>
/// Warning message class.
/// </summary>
internal class WarningMessage : Message
{
    /// <summary>
    /// Background color of the warning message.
    /// </summary>
    public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Yellow;

    /// <summary>
    /// Dictionary of warning messages.
    /// </summary>
	public override Dictionary<string, string> MessageContent { get; } = new()
	{
        // TODO
	};
}

