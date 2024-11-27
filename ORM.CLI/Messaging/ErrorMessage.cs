namespace MyORM.CLI.Messaging;

/// <summary>
/// Error message class.
/// </summary>
internal class ErrorMessage : Message
{
    /// <summary>
    /// Background color of the error message.
    /// </summary>
    public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Red;

    /// <summary>
    /// Dictionary of error messages.
    /// </summary>
    public override Dictionary<string, string> MessageContent { get; } = new()
	{
		{ "MissingMigrationName", "Missing the following arguments: Migration Name" },
		{ "InvalidInput", "Invalid input" },
		{ "Exception", "An exception has been thrown: "},
		{ "DataAccessLayerNotFound", "Class with DataAccessLayer attribute not found" },
	};
}

