﻿using CLI.Messaging;
using CLI.Methods;
using ORM;
using ORM.Abstract;
using ORM.Attributes;
using ORM.Common;

namespace CLI.Operations;

internal class Migration
{
    private static ILogger _logger;

    public Migration(ILogger logger)
    {
        _logger = logger;

		_logger.LogInfo("BuildStarted", null);
        try
        {
            Project.Build();
        }
        catch (Exception e)
        {
			_logger.LogError(e.Message);
            return;
        }
        _logger.LogInfo("BuildSucceeded", null);
    }

	public void Create(string input)
	{
        _logger.LogInfo("CheckingDirectory", new[] { "Migrations" });

        string directoryPath = "Migrations";
		if (!Directory.Exists(directoryPath))
		{
			_logger.LogInfo("CreatingDirectory", null);
			Directory.CreateDirectory(directoryPath);
			_logger.LogInfo("DirectoryCreated", new [] { Directory.GetCurrentDirectory() });
		}
		string currentDirectory = Directory.GetCurrentDirectory().Split('\\').Last();

		var types = AttributeHelpers.GetPropsByAttribute(typeof(Entity));
        _logger.LogInfo("ProcessingEntities", types.Select(x => x.ClassName).ToArray());

        // SNAPSHOT
		_logger.LogInfo("CheckingFile", new [] { "ModelSnapshot.cs" });
        string snapshotFile = Path.Combine(directoryPath, "ModelSnapshot.cs");

        // MIGRATION
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string filename = $"{timestamp}_{input}.cs";
        string filepath = Path.Combine(directoryPath, filename);

        _logger.LogInfo("ProducingMigration", null);
        string content = MigrationFactory.ProduceMigrationContent(types, currentDirectory, $"M{timestamp}_{input}", File.Exists(snapshotFile) ? File.ReadAllText(snapshotFile) : "");

        using (var stream = File.CreateText(filepath))
        {
            stream.WriteLine(content);
        }
        _logger.LogInfo("Done", null);

        if (File.Exists(snapshotFile))
		{
            string snapshotContent = SnapshotFactory.ProduceShapshotContent(types, currentDirectory);
			File.WriteAllText(snapshotFile, snapshotContent);
        }
		else
		{
            _logger.LogInfo("CreatingSnapshot", new[] { "" });
            string snapshotContent = SnapshotFactory.ProduceShapshotContent(types, currentDirectory);
			using (var snapshotStream = File.CreateText(snapshotFile))
                snapshotStream.WriteLine(snapshotContent);
            _logger.LogInfo("FileCreated", new[] { $"ModelSnapshot.cs in {directoryPath}" });
        }
    }

	public void ExecuteMigration(string methodName)
	{
		var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).First();
		var connectionString = dataAccessProps.Properties.First(x => x.Name == "ConnectionString").Value;

		Schema schema = new Schema(connectionString.ToString());

		var migrationProps = AttributeHelpers.GetPropsByAttribute(typeof(ORM.Attributes.Migration)).Last();
		var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(ORM.Attributes.Snapshot)).Last();

		if (schema.CheckIfTableExists("_MyORMMigrationsHistory"))
		{
			if (!schema.CheckIfTheLastRecord("_MyORMMigrationsHistory", "MigrationName", $"{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}"))
				schema.Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}', NOW())");
			else
			{
				Console.WriteLine($"{(methodName == "Up" ? "Provided migration already executed" : "Provided migration already reverted")}");
				return;
			}

            var method = migrationProps.Methods.First(x => x.Name == methodName);
            method.Invoke(migrationProps.Instance, new object[] { schema });
}
		else
		{
			schema.Execute($"CREATE TABLE _MyORMMigrationsHistory (Id INT NOT NULL AUTO_INCREMENT, MigrationName VARCHAR(255) NOT NULL, Date DATETIME NOT NULL, PRIMARY KEY (Id))");
			schema.Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}', NOW())");
            
            var method = snapshotProps.Methods.First(x => x.Name == "CreateDBFromSnapshot");
            method.Invoke(snapshotProps.Instance, new object[] { schema });
        }
    }
}