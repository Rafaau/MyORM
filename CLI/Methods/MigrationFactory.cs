namespace CLI.Methods;

internal static class MigrationFactory
{
	public static string ProduceMigrationContent(List<AttributeHelpers.ClassProps> types, string nameSpace, string migrationName, string snapshotContent)
	{
		string content = $"using ORM.Abstract;\r\nusing ORM.Attributes;\r\n\r\nnamespace {nameSpace}.Migrations;\r\n\r\n[Migration]\r\npublic partial class {migrationName} : AbstractMigration\r\n{{\r\n\tpublic override string GetDescription()\r\n\t{{\r\n\t\treturn \"\";\r\n\t}}\r\n\tpublic override void Up(Schema schema)\r\n\t{{";
		
		foreach (var type in types)
		{
			content = content.HandleEntityPropsForUp(type, snapshotContent);
		}

		content+= "\r\n\t}\r\n\tpublic override void Down(Schema schema)\r\n\t{";

		foreach (var type in types)
		{
			content = content.HandleEntityPropsForDown(type, snapshotContent);
		}

		content += "\r\n\t}\r\n}";

		return content;
	}

	internal static string HandleEntityPropsForUp(this string content, AttributeHelpers.ClassProps type, string snapshotContent)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		string propsString = "";
		int index = 1;

        foreach (var prop in type.Properties)
		{
			propsString = propsString.HandlePropertyOptions(prop);
			if (index != type.Properties.Count())
				propsString += ", ";
			index++;
		}

        if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
			content += $"\r\n\t\tschema.Execute(\"CREATE TABLE {tableName} ({propsString})\");";

		return content;
	}

	internal static string HandleEntityPropsForUp(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		string propsString = "";
		int index = 1;

		foreach (var prop in type.Properties)
		{
			propsString = propsString.HandlePropertyOptions(prop);
			if (index != type.Properties.Count())
				propsString += ", ";
			index++;
		}

		content += $"\r\n\t\tschema.Execute(\"CREATE TABLE {tableName} ({propsString})\");";
		return content;
	}

	private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type, string snapshotContent)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
			content += $"\r\n\t\tschema.Execute(\"DROP TABLE {tableName}\");";

		return content;
	}

	private static string HandlePropertyOptions(this string content, AttributeHelpers.Property prop)
	{
		if (prop.Attributes.Select(x => x.Name).Contains("PrimaryGeneratedColumn"))
			content += $"{prop.Name} INT AUTO_INCREMENT NOT NULL, PRIMARY KEY ({prop.Name})";
		else if (prop.Attributes.Select(x => x.Name).Contains("Column"))
		{
            switch (prop.Type.ToString())
			{
				case "System.Int32":
					content += $"{prop.Name} INT";
					break;
				case "System.String":
					content += $"{prop.Name} VARCHAR(255)";
					break;
				case "System.DateTime":
					content += $"{prop.Name} DATETIME";
					break;
				case "System.Boolean":
					content += $"{prop.Name} BOOLEAN";
					break;
				default:
					break;
			}
		}


		return content;
	}
}
