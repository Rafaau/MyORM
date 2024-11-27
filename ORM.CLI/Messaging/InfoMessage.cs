namespace MyORM.CLI.Messaging;

/// <summary>
/// Info message class.
/// </summary>
internal class InfoMessage : Message
{
    /// <summary>
    /// Background color of the info message.
    /// </summary>
    public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Black;

    /// <summary>
    /// Dictionary of info messages.
    /// </summary>
    public override Dictionary<string, string> MessageContent { get; } = new()
	{
		{ "CheckingDataAccessLayer", "Checking Data Access Layer..." },
		{ "CheckingDirectory", "Checking directory: " },
		{ "CreatingDirectory", "Migrations directory not found. Creating..." },
		{ "DirectoryCreated", "Created directory in: " },
		{ "ProcessingEntities", "Processing entities: \n" },
		{ "CheckingFile", "Checking file: " },
		{ "CreatingSnapshot", "Snapshot.cs not found. Creating..." },
		{ "FileCreated", "Created file: " },
		{ "ProducingMigration", "Producing migration..." },
		{ "Done", "Done." },
		{ "BuildStarted", "Build started..." },
		{ "BuildSucceeded", "Build succeeded."}
	};
}