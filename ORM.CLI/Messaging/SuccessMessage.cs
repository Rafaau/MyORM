namespace MyORM.CLI.Messaging;

internal class SuccessMessage : Message
{
	public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Green;

	public override Dictionary<string, string> MessageContent { get; } = new()
	{
		{ "MigrationCreated", "Migration has been created successfully." },
		{ "MigrationApplied", "Migration has been applied successfully." },
		{ "MigrationReverted", "Migration has been reverted successfully." }
	};
}