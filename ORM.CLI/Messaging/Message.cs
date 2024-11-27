namespace MyORM.CLI.Messaging;

/// <summary>
/// Abstract message class.
/// </summary>
internal abstract class Message
{
    /// <summary>
    /// Background color of the message.
    /// </summary>
    public abstract ConsoleColor BackgroundColor { get; }

    /// <summary>
    /// Method to invoke a message by specified key.
    /// </summary>
    /// <param name="messageKey">Message key</param>
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

    /// <summary>
    /// Method to invoke a message by specified key with additional arguments.
    /// </summary>
    /// <param name="messageKey"></param>
    /// <param name="args"></param>
    public virtual void InvokeMessage(string messageKey, string[]? args)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Console.BackgroundColor = BackgroundColor;
		Console.WriteLine($"{MessageContent[$"{messageKey}"]}{(args is not null ? string.Join("\n", args) : "")}");
		Console.ResetColor();
		Console.WriteLine("");
		args = null;
	}

    /// <summary>
    /// Method to invoke a custom message.
    /// </summary>
    /// <param name="message">Custom message</param>
    public virtual void InvokeCustomMessage(string message)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Console.BackgroundColor = BackgroundColor;
		Console.WriteLine($"{message}");
		Console.ResetColor();
		Console.WriteLine("");
	}

    /// <summary>
    /// Method to invoke an error message by specified exception.
    /// </summary>
    /// <param name="exception">Exception to log</param>
    public virtual void InvokeErrorMessage(Exception exception)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Console.BackgroundColor = ConsoleColor.Red;
		Console.WriteLine("");
		Console.WriteLine("");
		Console.WriteLine($"{exception.Message}");
		Console.WriteLine($"{exception.StackTrace}");
		Console.WriteLine($"{exception.Source}");
		Console.ResetColor();
		Console.WriteLine("");
	}

    /// <summary>
    /// Abstract dictionary of message content.
    /// </summary>
    public abstract Dictionary<string, string> MessageContent { get; }
}

