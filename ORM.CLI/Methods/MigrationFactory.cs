using MyORM.Attributes;
using MyORM.Common.Methods;
using MyORM.Enums;
using MyORM.Models;

namespace MyORM.CLI.Methods;

internal static class MigrationFactory
{
	public static string ProduceMigrationContent(List<AttributeHelpers.ClassProps> types, string nameSpace, string migrationName, string snapshotContent)
	{
		var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(Snapshot))?.LastOrDefault();
		var method = snapshotProps?.Methods.First(x => x.Name == "GetModelsStatements");
		var modelStatements = (List<ModelStatement>)method?.Invoke(snapshotProps?.Instance, new object[] { })!;

		string content = 
			$"using MyORM.Abstract;\r\n" + 
			$"using MyORM.Attributes;\r\n\r\n" + 
			$"namespace {nameSpace}.Migrations;\r\n\r\n" +
			$"[Migration]\r\n" + 
			$"public partial class {migrationName} : AbstractMigration\r\n{{\r\n\t" + 
			$"public override string GetDescription()\r\n\t{{\r\n\t\t" + 
			$"return \"\";\r\n\t}}\r\n\t" + 
			$"public override void Up(DbHandler dbHandler)\r\n\t{{";

		if (snapshotContent != "")
		{
			foreach (var type in types)
			{
				content = content.HandleEntityPropsForUp(type, snapshotContent, modelStatements.SingleOrDefault(x => x.Name == type.ClassName));
			}
		}
		else
		{
			foreach (var type in types)
			{
				content = content.HandleEntityPropsForUp(type);
			}

			foreach (var type in types)
			{
				content = content.HandleEntityRelationPropsForUp(type);
			}
		}

		content += "\r\n\t}\r\n\tpublic override void Down(DbHandler dbHandler)\r\n\t{";

		if (snapshotContent != "")
		{
			foreach (var type in types)
			{
				content = content.HandleEntityPropsForDown(type, snapshotContent, modelStatements.SingleOrDefault(x => x.Name == type.ClassName));
			}
		}
		else
		{
			foreach (var type in types)
			{
				content = content.HandleEntityPropsForDown(type);
			}
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
			content += $"\r\n\t\tdbHandler.Execute(\"CREATE TABLE {tableName} ({propsString})\");";
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

		var properties =
			type.Properties.Where(x => x.Attributes.Any(attribute => 
			!attribute.FullName!.Contains("OneToOne") &&
			!attribute.FullName!.Contains("ManyToOne") &&
			!attribute.FullName!.Contains("OneToMany")
		));


		foreach (var prop in properties)
		{
			propsString += HandlePropertyOptions(prop, Operation.Create);
			if (index != properties.Count())
				propsString += ", ";
			index++;
		}

		content += $"\r\n\t\tdbHandler.Execute(\"CREATE TABLE {tableName} ({propsString})\");";

		return content;
	}

	internal static string HandleEntityRelationPropsForUp(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		string propsString = "";
		int index = 1;

		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => 
			x.FullName!.Contains("OneToOne") ||
			x.FullName!.Contains("ManyToOne")))
		)
		{
			string relationshipString = GetRelationship(prop) == Relationship.Optional 
				? "NULL" 
				: "NOT NULL, " +
				$"ADD FOREIGN KEY ({prop.Name}Id) " +
				$"REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)" +
				(GetCascadeOption(prop) ? " ON DELETE CASCADE" : "");

			string unique = !prop.Attributes.Any(x => x.FullName!.Contains("ManyToOne")) ? " UNIQUE" : "";

			propsString += $"ADD {prop.Name}Id INT{unique} {relationshipString}";
				
		}

		content += $"\r\n\t\tdbHandler.Execute(\"ALTER TABLE {tableName} {propsString}\");";

		return content;
	}

	private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		content += $"\r\n\t\tdbHandler.Execute(\"DROP TABLE {tableName}\");";
		return content;
	}

	private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement? modelStatement)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
			content += $"\r\n\t\tdbHandler.Execute(\"DROP TABLE {tableName}\");";
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
		{
			string relationshipString = GetRelationship(prop) == Relationship.Optional
				? "NULL"
				: "NOT NULL, " +
				$"ADD FOREIGN KEY ({prop.Name}Id) " +
				$"REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)" +
				(GetCascadeOption(prop) ? " ON DELETE CASCADE" : "");

			content += $"{prop.ColumnName} INT UNIQUE {relationshipString}";
		}
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("ManyToOne"))
			content += $"{prop.ColumnName} INT, {(operation == Operation.Alter ? "ADD" : "")} FOREIGN KEY ({prop.ColumnName}) REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)";
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

		foreach (var prop in type.Properties.Where(x => !x.Attributes.Any(y => y.FullName!.Contains("OneToMany"))))
		{
			if (!modelStatement.Columns.Any(x => x.PropertyName == prop.Name))
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
			if (!type.Properties.Select(x => x.Name).Contains(column.PropertyName))
			{
				if (index > 1)
					propsString += ", ";

				if (method == Method.Up)
				{
					propsString += "DROP COLUMN ";
					propsString += column.PropertyName;
				}
				else
				{
					propsString += "ADD ";
					propsString += column.PropertyName + " " + column.Type;
				}

				index++;
			}
		}

		if (propsString != "")
			content += $"\r\n\t\tdbHandler.Execute(\"ALTER TABLE {tableName} {propsString}\");";

		return content;
	}

	private static bool GetCascadeOption(AttributeHelpers.Property property)
	{
		bool cascade = false;

		var baseProps = AttributeHelpers.GetPropsByModel(property.Type);

		var cascadeAttr = baseProps.Properties
			.Find(x => x.Type.Name == property.ParentClass.ClassName)?.AttributeProps?
			.FirstOrDefault(x => x.Key == "Cascade").Value;

        if (cascadeAttr != null)
		{
			cascade = (bool)cascadeAttr;
		}
			
		return cascade;
	}

	private static Relationship GetRelationship(AttributeHelpers.Property property)
	{
		var relationshipAttr = property.AttributeProps.FirstOrDefault(x => x.Key == "Relationship").Value;
		return relationshipAttr != null ? (Relationship)relationshipAttr : Relationship.Mandatory;
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

	internal enum Destination
	{
		Migration,
		Snapshot
	}
}
