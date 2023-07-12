using CLI.Methods;
using ORM;
using ORM.Abstract;
using ORM.Attributes;

namespace CLI.Operations;

internal class Migration
{
	public static void Create(string input)
	{
		string directoryPath = "Migrations";
		if (!Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
		string currentDirectory = Directory.GetCurrentDirectory().Split('\\').Last();

		var types = AttributeHelpers.GetPropsByAttribute(typeof(Entity));

		// SNAPSHOT
		string snapshotFile = Path.Combine(directoryPath, "Snapshot.cs");

		if (File.Exists(snapshotFile))
		{
			string snapshotContent = SnapshotFactory.ProduceShapshotContent(types, currentDirectory);
			File.WriteAllText(snapshotFile, snapshotContent);
		}
		else
		{
			string snapshotContent = SnapshotFactory.ProduceShapshotContent(types, currentDirectory);
			using (var snapshotStream = File.CreateText(snapshotFile))
			{
				snapshotStream.WriteLine(snapshotContent);
			}
		}

		// MIGRATION
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		string filename = $"{timestamp}_{input}.cs";
		string filepath = Path.Combine(directoryPath, filename);

        string content = MigrationFactory.ProduceMigrationContent(types, currentDirectory, $"M{timestamp}_{input}", File.ReadAllText(snapshotFile));

		using (var stream = File.CreateText(filepath))
		{
			stream.WriteLine(content);
		}
	}

	public static void ExecuteMigration(string methodName)
	{
		var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).First();
		var connectionString = dataAccessProps.Properties.First(x => x.Name == "ConnectionString").Value;

		Schema schema = new Schema(connectionString.ToString());

		var migrationProps = AttributeHelpers.GetPropsByAttribute(typeof(ORM.Attributes.Migration)).Last();

		if (schema.CheckIfTableExists("_MyORMMigrationsHistory"))
		{
			if (!schema.CheckIfTheLastRecord("_MyORMMigrationsHistory", "MigrationName", $"{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}"))
				schema.Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}', NOW())");
			else
			{
				Console.WriteLine($"{(methodName == "Up" ? "Provided migration already executed" : "Provided migration already reverted")}");
				return;
			}
		}
		else
		{
			schema.Execute($"CREATE TABLE _MyORMMigrationsHistory (Id INT NOT NULL AUTO_INCREMENT, MigrationName VARCHAR(255) NOT NULL, Date DATETIME NOT NULL, PRIMARY KEY (Id))");
			schema.Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationProps.ClassName}{(methodName == "Down" ? "_revert" : "")}', NOW())");
		}

		var method = migrationProps.Methods.First(x => x.Name == methodName);
		method.Invoke(migrationProps.Instance, new object[] { schema });
	}
}
