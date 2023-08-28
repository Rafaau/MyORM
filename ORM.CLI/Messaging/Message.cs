namespace CLI.Messaging;

internal abstract class Message
{
    public abstract ConsoleColor BackgroundColor { get; }

    public virtual void InvokeMessage(string messageKey)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = BackgroundColor;
        Console.WriteLine("");
        Console.WriteLine(
            $"\n {MessageContent[$"{messageKey}"]}"
                .PadLeft(40)
                .PadRight(40)
        );
        Console.ResetColor();
        Console.WriteLine("");
    }

    public virtual void InvokeMessage(string messageKey, string[]? args)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = BackgroundColor;
        Console.WriteLine($"{MessageContent[$"{messageKey}"]}{(args is not null ? string.Join(",\n", args) : "")}");
        Console.ResetColor();
        args = null;
    }

    public abstract Dictionary<string, string> MessageContent { get; }
}

