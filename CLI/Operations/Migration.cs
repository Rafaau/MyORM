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

		// MIGRATION
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		string filename = $"{timestamp}_{input}.cs";
		string filepath = Path.Combine(directoryPath, filename);

		var types = AttributeHelpers.GetPropsByAttribute(typeof(Entity));
        string content = MigrationFactory.ProduceMigrationContent(types, currentDirectory, $"M{timestamp}_{input}");

		using (var stream = File.CreateText(filepath))
		{
			stream.WriteLine(content);
		}

		// SNAPSHOT
		string snapshotFile = Path.Combine(directoryPath, "Snapshot.cs");

		if (File.Exists(snapshotFile))
		{
			string snapshotContent = File.ReadAllText(snapshotFile);
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
	}

	public static void Test()
	{
		var types = AttributeHelpers.GetPropsByAttribute(typeof(Entity));
		foreach (var type in types)
		{
			Console.WriteLine($"Class:{type.ClassName} | Key:{type.AttributeProps.First().Key}, Value:{type.AttributeProps.First().Value}");
            Console.WriteLine(type.Properties.Count());

            foreach (var field in type.Properties)
			{
				string attributes = string.Join(", ", field.Attributes.Select(x => x.Name));
				Console.WriteLine($"{field.Type.ToString()} {field.Name} - {attributes}");
			}

            Console.WriteLine("------------------------------");
        }
	}

	public static void ExecuteMigration(string methodName)
	{
		var dataAccessProps = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).First();
		var connectionString = dataAccessProps.Properties.First(x => x.Name == "ConnectionString").Value;

		Schema schema = new Schema(connectionString.ToString());

		var migrationProps = AttributeHelpers.GetPropsByAttribute(typeof(ORM.Attributes.Migration)).Last();
		var method = migrationProps.Methods.First(x => x.Name == methodName);
		method.Invoke(migrationProps.Instance, new object[] { schema });
	}
}
