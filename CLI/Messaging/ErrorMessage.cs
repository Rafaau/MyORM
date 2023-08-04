namespace CLI.Messaging;

internal class ErrorMessage : Message
{
    public override ConsoleColor BackgroundColor { get; } = ConsoleColor.Red;

    public override Dictionary<string, string> MessageContent { get; } = new()
    {
        { "MissingMigrationName", "Missing the following arguments: Migration Name" },
        { "InvalidInput", "Invalid input" }
    };
}

