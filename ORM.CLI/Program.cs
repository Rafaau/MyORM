using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyORM.CLI.Messaging.Interfaces;
using MyORM.CLI.Messaging.Services;
using MyORM.CLI.Operations;
using MyORM.CLI.Enums;

/// <summary>
/// Main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// Main method.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    private static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddTransient<ILogger, Logger>();
                services.AddTransient<Migration>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger>();
        var migration = host.Services.GetRequiredService<Migration>();

        var command = args.AsQueryable().FirstOrDefault();

        if (command == null)
        {
            Console.WriteLine("Input: ");
            command = Console.ReadLine()!.Trim();
        }

        if (command.StartsWith("migration:create"))
        {
            if (args.Length < 2)
                logger.LogError("Missing Migration Name");
			else
			{
                try
                {
					migration.Create(args[1]);
					logger.LogSuccess("MigrationCreated");
				}
				catch (Exception e)
                {
                    logger.LogError("Exception", new[] { e.Message, e.Source });
                }
            }

        }
        else if (command.StartsWith("migration:migrate"))
        {
            try
            {
                migration.ExecuteMigration(Method.Up);
                logger.LogSuccess("MigrationApplied");
            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                var file = frame.GetFileName();
                logger.LogError("Exception", new[] { e.Message, e.Source, $"File: {file}", $"Line: {line}" });
            }

        }
        else if (command.StartsWith("migration:revert"))
        {
            migration.ExecuteMigration(Method.Down);
            logger.LogSuccess("MigrationReverted");
        }
        else
        {
            logger.LogError("Invalid Input");
        }
    }
}
