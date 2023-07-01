using CLI.Methods;
using ORM;
using ORM.Abstract;
using ORM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			Console.WriteLine(type.ClassName + " " + type.AttributeProps.First().Key + " - " + type.AttributeProps.First().Value);
            Console.WriteLine(type.FieldProps.Count());

            foreach (var field in type.FieldProps)
			{
				Console.WriteLine(field.FieldType.ToString() + " " + field.FieldName);
			}

            Console.WriteLine("------------------------------");
        }
	}

	public static void Migrate()
	{
		var props = AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer)).First(); 
		foreach (var prop in props.FieldProps)
		{
			Console.WriteLine(prop.FieldName + " " + prop.FieldValue);
		}
	}
}
