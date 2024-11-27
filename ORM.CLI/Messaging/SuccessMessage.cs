namespace MyORM.CLI.Messaging;

/// <summary>
/// Success message class.
/// </summary>
internal class SuccessMessage : Message
{
    /// <summary>
    /// Background color of the success message.
    /// </summary>
    public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Green;

    /// <summary>
    /// Dictionary of success messages.
    /// </summary>
    public override Dictionary<string, string> MessageContent { get; } = new()
	{
		{ "MigrationCreated", "Migration has been created successfully." },
		{ "MigrationApplied", "Migration has been applied successfully." },
		{ "MigrationReverted", "Migration has been reverted successfully." }
	};
}