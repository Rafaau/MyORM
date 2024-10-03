using MyORM.Abstract;
using MyORM.Attributes;
using MyORM.CLI.Messaging.Interfaces;
using MyORM.CLI.Methods;
using MyORM.Common.Methods;
using System.Reflection;

namespace MyORM.CLI.Operations;

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
			_logger.LogError(e);
			return;
		}
		_logger.LogInfo("BuildSucceeded", null);
	}

	public void Create(string input)
	{
		_logger.LogInfo("CheckingDataAccessLayer", null);
		var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).FirstOrDefault();
		
		if (dataAccessProps == null)
		{
			_logger.LogError("DataAccessLayerNotFound", null);
			throw new Exception("DataAccessLayer not found");
		}

		var options = (Options)dataAccessProps.Properties.First(x => x.Name == "Options").Value;

		string directoryPath = options.MigrationsAssembly == ""
			? Path.Combine(Directory.GetCurrentDirectory(), "Migrations")
			: Path.Combine(options.GetMigrationsMainDirectory(), "Migrations");

		string nameSpace = options.MigrationsAssembly == ""
			? Directory.GetCurrentDirectory().Split('\\').Last()
			: options.MigrationsAssembly;

		if (!Directory.Exists(directoryPath))
		{
			_logger.LogInfo("CreatingDirectory", null);
			Directory.CreateDirectory(directoryPath);
			_logger.LogInfo("DirectoryCreated", new[] { Directory.GetCurrentDirectory() });
		}

		var types = AttributeHelpers.GetPropsByAttribute(typeof(Entity), options.GetEntitiesAssembly());
		_logger.LogInfo("ProcessingEntities", types.Select(x => x.ClassName).ToArray());

		// SNAPSHOT
		_logger.LogInfo("CheckingFile", new[] { "ModelSnapshot.cs" });
		string snapshotFile = Path.Combine(directoryPath, "ModelSnapshot.cs");
		string snapshotContent = "";

		// MIGRATION
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		string filename = $"{timestamp}_{input}.cs";
		string filepath = Path.Combine(directoryPath, filename);

		_logger.LogInfo("ProducingMigration", null);
		string content = MigrationFactory.ProduceMigrationContent(types, nameSpace, $"M{timestamp}_{input}", File.Exists(snapshotFile) ? File.ReadAllText(snapshotFile) : "");

		using (var stream = File.CreateText(filepath))
		{
			stream.WriteLine(content);
		}

		if (File.Exists(snapshotFile))
		{
			snapshotContent = SnapshotFactory.ProduceShapshotContent(types, nameSpace);
			File.WriteAllText(snapshotFile, snapshotContent);
		}
		else
		{
			_logger.LogInfo("CreatingSnapshot", new[] { "" });
			snapshotContent = SnapshotFactory.ProduceShapshotContent(types, nameSpace);
			using (var snapshotStream = File.CreateText(snapshotFile))
				snapshotStream.WriteLine(snapshotContent);
			_logger.LogInfo("FileCreated", new[] { $"ModelSnapshot.cs in {directoryPath}" });

		}

		_logger.LogInfo("Done", null);
	}

	public async void ExecuteMigration(string methodName)
	{
		try
		{
			var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).FirstOrDefault();
			if (dataAccessProps == null)
			{
				throw new Exception("DataAccessLayer not found");
			}

			var options = (Options)dataAccessProps.Properties.Find(x => x.Name == "Options").Value;

			var connectionString = dataAccessProps.Properties.Find(x => x.Name == "ConnectionString")!.Value;

			DbHandler dbHandler = new DbHandler(connectionString.ToString());

			var migrationProps = AttributeHelpers.GetPropsByAttribute(typeof(Attributes.Migration), options.GetMigrationsAssembly()).LastOrDefault();
			var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(Snapshot), options.GetMigrationsAssembly()).LastOrDefault();

			if (dbHandler.CheckIfTableExists("_MyORMMigrationsHistory"))
			{
				if (migrationProps == null)
				{
					throw new Exception("Migration not found");
				}

				bool doesExist = false;

				if (migrationProps != null 
					&& !dbHandler.CheckTheLastRecord("_MyORMMigrationsHistory", "MigrationName", $"{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}"))
				{
					doesExist = true;
				}
				else
				{
					Console.WriteLine($"{(methodName == "Up" ? "Provided migration already executed" : "Provided migration already reverted")}");
					return;
				}

				var method = migrationProps.Methods.First(x => x.Name == methodName);
				method.Invoke(migrationProps.Instance, new object[] { dbHandler });

				if (doesExist)
					dbHandler.Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}', NOW())");
			}
			else
			{
				if (snapshotProps == null)
				{
					throw new Exception("Snapshot not found");
				}

				if (migrationProps != null)
				{
					dbHandler.Execute($"CREATE TABLE _MyORMMigrationsHistory (Id INT NOT NULL AUTO_INCREMENT, MigrationName VARCHAR(255) NOT NULL, Date DATETIME NOT NULL, PRIMARY KEY (Id))");
					dbHandler.Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}', NOW())");
				}

				var method = snapshotProps.Methods.First(x => x.Name == "CreateDBFromSnapshot");
				method.Invoke(snapshotProps.Instance, [dbHandler]);
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e);
			return;
		}
	}
}
