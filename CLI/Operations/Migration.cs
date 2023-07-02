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
		string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
		string filename = $"{timestamp}_{input}.cs";
		string filepath = Path.Combine(directoryPath, filename);

		using (var stream = File.CreateText(filepath))
		{
			stream.WriteLine("using ORM");
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

		Console.WriteLine(connectionString);

		Schema schema = new Schema(connectionString.ToString());

		var migrationProps = AttributeHelpers.GetPropsByAttribute(typeof(ORM.Attributes.Migration)).First();
		var method = migrationProps.Methods.First(x => x.Name == methodName);
		method.Invoke(migrationProps.Instance, new object[] { schema });
	}
}
