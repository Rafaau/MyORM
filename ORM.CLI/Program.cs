using System.Diagnostics;
using CLI.Messaging;
using CLI.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// 1. dotnet pack
// 2. dotnet tool install / update --global --add-source ./nupkg orm.cli
public class Program
{
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
                logger.LogError("MissingMigrationName");
            else
            {
                migration.Create(args[1]);
                logger.LogSuccess("MigrationCreated");
            }

        }
        else if (command.StartsWith("migration:migrate"))
        {
            try
            {
                migration.ExecuteMigration("Up");
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
            migration.ExecuteMigration("Down");
            logger.LogSuccess("MigrationReverted");
        }
        else
        {
            logger.LogError("InvalidInput");
        }
    }
}
