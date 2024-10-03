namespace MyORM.CLI.Messaging;

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
		Console.WriteLine($"{MessageContent[$"{messageKey}"]}{(args is not null ? string.Join("\n", args) : "")}");
		Console.ResetColor();
		Console.WriteLine("");
		args = null;
	}

	public virtual void InvokeCustomMessage(string message)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Console.BackgroundColor = BackgroundColor;
		Console.WriteLine($"{message}");
		Console.ResetColor();
		Console.WriteLine("");
	}

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

	public abstract Dictionary<string, string> MessageContent { get; }
}

