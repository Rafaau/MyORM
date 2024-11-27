using MyORM.CLI.Enums;
using MyORM.CLI.Messaging.Interfaces;
using MyORM.CLI.Methods;
using MyORM.DBMS;
using MyORM.Methods;
using System.Configuration;

namespace MyORM.CLI.Operations;

/// <summary>
/// Migration service class.
/// </summary>
internal class Migration
{
    /// <summary>
    /// Logger instance.
    /// </summary>
    private static ILogger _logger;

    /// <summary>
    /// Constructor for the Migration class <see cref="Migration"/>.
    /// </summary>
    /// <param name="logger">Logger instance</param>
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

    /// <summary>
    /// Creates a new migration.
    /// </summary>
    /// <param name="input">Command input</param>
    /// <exception cref="Exception">Exception thrown when DataAccessLayer not found</exception>
    public void Create(string input)
	{
		_logger.LogInfo("CheckingDataAccessLayer", null);
		var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).FirstOrDefault();
		
		if (dataAccessProps == null)
		{
			_logger.LogError("DataAccessLayerNotFound", null);
			throw new Exception("DataAccessLayer not found");
		}

		Options options = (Options)dataAccessProps.Properties.First(x => x.Name == "Options").Value;

		string entitiesAssemblyPath;
		string directoryPath;
		string nameSpace;

		#if DEBUG
			entitiesAssemblyPath = ConfigurationManager.AppSettings["EntitiesAssemblyPath"];
			directoryPath = "D:\\repos\\ORM\\Test\\Migrations";
			nameSpace = "Test";
		#else
			entitiesAssemblyPath = options.GetEntitiesAssembly();
			directoryPath = options.MigrationsAssembly == ""
				? Path.Combine(Directory.GetCurrentDirectory(), "Migrations")
				: Path.Combine(options.GetMigrationsMainDirectory(), "Migrations");

			nameSpace = options.MigrationsAssembly == ""
				? Directory.GetCurrentDirectory().Split('\\').Last()
				: options.MigrationsAssembly;
		#endif

		if (!Directory.Exists(directoryPath))
		{
			_logger.LogInfo("CreatingDirectory", null);
			Directory.CreateDirectory(directoryPath);
			_logger.LogInfo("DirectoryCreated", new[] { Directory.GetCurrentDirectory() });
		}

		var types = AttributeHelpers.GetPropsByAttribute(typeof(Entity), entitiesAssemblyPath);
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
		string content = MigrationFactory.ProduceMigrationContent(
			types, 
			nameSpace, 
			$"M{timestamp}_{input}", 
			File.Exists(snapshotFile) 
				? File.ReadAllText(snapshotFile) 
				: "",
			options.Database);

		using (var stream = File.CreateText(filepath))
		{
			stream.WriteLine(content);
		}

		snapshotContent = SnapshotFactory.ProduceShapshotContent(types, nameSpace, options.Database);

		if (File.Exists(snapshotFile))
		{
			File.WriteAllText(snapshotFile, snapshotContent);
		}
		else
		{
			_logger.LogInfo("CreatingSnapshot", new[] { "" });
			using (var snapshotStream = File.CreateText(snapshotFile))
				snapshotStream.WriteLine(snapshotContent);
			_logger.LogInfo("FileCreated", new[] { $"ModelSnapshot.cs in {directoryPath}" });

		}

		_logger.LogInfo("Done", null);
	}

    /// <summary>
    /// Executes the migration.
    /// </summary>
    /// <param name="method">Migration method</param>
    public async void ExecuteMigration(Method method)
	{
		try
		{
			var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).FirstOrDefault();
			if (dataAccessProps == null)
			{
				throw new Exception("DataAccessLayer not found");
			}

			Options options = (Options)dataAccessProps.Properties.Find(x => x.Name == "Options").Value;

			AccessLayer accessLayer = (AccessLayer)dataAccessProps.Instance;

			var connectionString = dataAccessProps.Properties.Find(x => x.Name == "ConnectionString")!.Value;

			DbHandler dbHandler = new DbHandler(accessLayer);

			ScriptBuilder.Database = options.Database;

			try
			{
				dbHandler.BeginTransaction();

				#if DEBUG
					string migrationsAssemblyPath = ConfigurationManager.AppSettings["MigrationsPath"]!;
				#else
					string migrationsAssemblyPath = options.GetMigrationsAssembly();
				#endif

				var migrationProps = AttributeHelpers.GetPropsByAttribute(typeof(MyORM.Migration), migrationsAssemblyPath).LastOrDefault();
				var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(MyORM.Snapshot), migrationsAssemblyPath).LastOrDefault();

				if (dbHandler.CheckIfTableExists("_MyORMMigrationsHistory"))
				{
					if (migrationProps == null)
					{
						throw new Exception("Migration not found");
					}

					bool doesExist = false;

					if (migrationProps != null
						&& !dbHandler.CheckTheLastRecord("_MyORMMigrationsHistory", "MigrationName", $"{migrationProps.ClassName}{(method == Method.Down ? "_revert" : "")}"))
					{
						doesExist = true;
					}
					else
					{
						Console.WriteLine($"{(method == Method.Up ?
							"Provided migration already executed" :
							"Provided migration already reverted")}");
						return;
					}

					var methodInfo = migrationProps.Methods.First(x => x.Name == method.ToString());
					methodInfo.Invoke(migrationProps.Instance, new object[] { dbHandler });

					if (doesExist)
						dbHandler.InsertMigrationRecord(migrationProps.ClassName, method == Method.Down);
				}
				else
				{
					if (snapshotProps == null)
					{
						throw new Exception("Snapshot not found");
					}

					if (migrationProps != null)
					{
						dbHandler.CreateMigrationHistoryTable();
						dbHandler.InsertMigrationRecord(migrationProps.ClassName, method == Method.Down);
					}

					var methodInfo = snapshotProps.Methods.First(x => x.Name == "CreateDBFromSnapshot");
					methodInfo.Invoke(snapshotProps.Instance, [dbHandler]);
				}

				dbHandler.CommitTransaction();
			}
			catch (Exception e)
			{
				dbHandler.RollbackTransaction();
				throw e;
			}
		}
		catch (Exception e)
		{
			throw e;
		}
	}
}
