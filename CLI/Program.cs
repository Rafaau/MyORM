using CLI.Operations;

public class Program
{
	private static void Main(string[] args)
	{
		var command = args.AsQueryable().FirstOrDefault();

		if (command == null)
		{
            Console.WriteLine("Input: ");
			command = Console.ReadLine()!.Trim();
        }

		if (command.StartsWith("migration:create"))
		{
			//Migration.Test();
			Migration.Create(args[1]);
		}
		else if (command.StartsWith("migration:migrate"))
		{
			Migration.ExecuteMigration("Up");
		}
		else if (command.StartsWith("migration:revert"))
		{
			Migration.ExecuteMigration("Down");
		}
		else
		{
			Console.WriteLine("Invalid input");
		}
    }
}
