using CLI.Methods;
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

	public static void Migrate()
	{
		AttributeHelpers.GetPropsByAttribute(typeof(DataAccessLayer));
	}
}
