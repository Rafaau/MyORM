using System.Text.RegularExpressions;
using ORM.Common;
using ORM.Models;

namespace CLI.Methods;

internal static class MigrationFactory
{
	public static string ProduceMigrationContent(List<AttributeHelpers.ClassProps> types, string nameSpace, string migrationName, string snapshotContent)
	{
        var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(ORM.Attributes.Snapshot))?.Last();
        var method = snapshotProps?.Methods.First(x => x.Name == "GetModelsStatements");
        var modelStatements = (List<ModelStatement>) method?.Invoke(snapshotProps?.Instance, new object[] { })!;

        string content = $"using ORM.Abstract;\r\nusing ORM.Attributes;\r\n\r\nnamespace {nameSpace}.Migrations;\r\n\r\n[Migration]\r\npublic partial class {migrationName} : AbstractMigration\r\n{{\r\n\tpublic override string GetDescription()\r\n\t{{\r\n\t\treturn \"\";\r\n\t}}\r\n\tpublic override void Up(Schema schema)\r\n\t{{";
		
		foreach (var type in types)
		{
			content = content.HandleEntityPropsForUp(type, snapshotContent, modelStatements.SingleOrDefault(x => x.Name == type.ClassName));
		}

		content+= "\r\n\t}\r\n\tpublic override void Down(Schema schema)\r\n\t{";

		foreach (var type in types)
		{
			content = content.HandleEntityPropsForDown(type, snapshotContent, modelStatements.SingleOrDefault(x => x.Name == type.ClassName));
		}

		content += "\r\n\t}\r\n}";

		return content;
	}

	internal static string HandleEntityPropsForUp(this string content, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement? modelStatement)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		string propsString = "";
		int index = 1;

        foreach (var prop in type.Properties)
		{
			propsString += HandlePropertyOptions(prop, Operation.Create);
			if (index != type.Properties.Count())
				propsString += ", ";
			index++;
		}

        if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
			content += $"\r\n\t\tschema.Execute(\"CREATE TABLE {tableName} ({propsString})\");";
		else
			content = content.HandleEntityChanges(tableName, type, snapshotContent, modelStatement, Method.Up);

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
			propsString += HandlePropertyOptions(prop, Operation.Create);
			if (index != type.Properties.Count())
				propsString += ", ";
			index++;
		}

		content += $"\r\n\t\tschema.Execute(\"CREATE TABLE {tableName} ({propsString})\");";

		return content;
	}

	private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement? modelStatement)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
			content += $"\r\n\t\tschema.Execute(\"DROP TABLE {tableName}\");";
        else
            content = content.HandleEntityChanges(tableName, type, snapshotContent, modelStatement, Method.Down);

		return content;
	}

	internal static string HandlePropertyOptions(AttributeHelpers.Property prop, Operation operation)
    {
        string content = "";

		if (prop.Attributes.Select(x => x.Name).Contains("PrimaryGeneratedColumn"))
			content += $"{prop.Name} INT AUTO_INCREMENT NOT NULL, PRIMARY KEY ({prop.Name})";
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("OneToOne"))
            content += $"{prop.ColumnName} INT UNIQUE, {(operation == Operation.Alter ? "ADD" : "")} FOREIGN KEY ({prop.ColumnName}) REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)";
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

    private static string HandleEntityChanges(this string content, string tableName, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement modelStatement, Method method)
    {
        string propsString = "";
        int index = 1;

        foreach (var prop in type.Properties)
        {
            if (!modelStatement.Columns.Any(x => x.Name == prop.Name))
            {
				if (index > 1)
					propsString += ", ";

				if (method == Method.Up)
                {
                    propsString += "ADD ";
                    propsString += HandlePropertyOptions(prop, Operation.Alter);
                }
                else
                {
                    propsString += "DROP COLUMN ";
                    propsString += prop.ColumnName;
                }

                index++;
            }
        }

        foreach (var column in modelStatement.Columns)
        {
            if (!type.Properties.Select(x => x.Name).Contains(column.Name))
            {
                if (index > 1)
                    propsString += ", ";

                if (method == Method.Up)
                {
                    propsString += "DROP COLUMN ";
                    propsString += column.Name;
                }
                else
                {
                    propsString += "ADD ";
                    propsString += column.Name + " " + column.Type;
                }

                index++;
            }
        }

		if (propsString != "")
            content += $"\r\n\t\tschema.Execute(\"ALTER TABLE {tableName} {propsString}\");";

        return content;
    }

    private enum Method
    {
		Up,
		Down
    }

    internal enum Operation
    {
		Create,
		Alter
    }
}
