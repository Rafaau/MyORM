using MyORM.Methods;
using MyORM.Models;
using MyORM.DBMS;
using Microsoft.Identity.Client;

namespace MyORM.CLI.Methods;

internal static class MigrationFactory
{
	public static string ProduceMigrationContent(
		List<AttributeHelpers.ClassProps> types, 
		string nameSpace, 
		string migrationName, 
		string snapshotContent,
		Database databaseManagementSystem)
	{
		ScriptBuilder.Database = databaseManagementSystem;

		var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(Snapshot))?.LastOrDefault();
		var method = snapshotProps?.Methods.First(x => x.Name == "GetModelsStatements");
		var modelStatements = (List<ModelStatement>)method?.Invoke(snapshotProps?.Instance, new object[] { })!;

		string content = 
			$"using MyORM;\r\n" +
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

			foreach (var type in types)
			{
				content = content.HandleManyToManyForUp(type, snapshotContent);
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

		List<AttributeHelpers.Property> properties = 
			type.Properties.Where(x => !x.Attributes.Any(x => x.FullName.Contains("ManyToMany"))).ToList();

		foreach (var prop in properties)
		{
			propsString += HandlePropertyOptions(prop, Operation.Create);
			if (index != properties.Count())
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

		List<string> propsString = new();

		var properties =
			type.Properties.Where(x => x.Attributes.Any(attribute => 
			!attribute.FullName!.Contains("OneToOne") &&
			!attribute.FullName!.Contains("ManyToOne") &&
			!attribute.FullName!.Contains("OneToMany") &&
			!attribute.FullName!.Contains("ManyToMany")
		));


		foreach (var prop in properties)
			propsString.Add($"\r\n\t\t\t\t{HandlePropertyOptions(prop, Operation.Create)}");

		content += 
			$"\r\n\t\tdbHandler.Execute(" +
			$"\r\n\t\t\t@\"CREATE TABLE {type.TableName} (" +
			$"{string.Join(", ", propsString)}" +
			$"\r\n\t\t\t)\"" +
			$"\r\n\t\t);";

		return content;
	}

	internal static string HandleEntityRelationPropsForUp(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");

		List<string> propsString = new();

		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => 
			x.FullName!.Contains("OneToOne") ||
			x.FullName!.Contains("ManyToOne")))
		)
		{
			string relationshipString = GetRelationship(prop) == Relationship.Optional 
				? "NULL" 
				: "NOT NULL, " +
				ScriptBuilder.BuildForeignKey(type.TableName, prop) +
				$"REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)" +
				(GetCascadeOption(prop) ? " ON DELETE CASCADE" : "");

			string unique = !prop.Attributes.Any(x => x.FullName!.Contains("ManyToOne")) ? " UNIQUE" : "";

			propsString.Add($"\r\n\t\t\t\tADD {prop.Name}Id INT{unique} {relationshipString}");
				
		}

		if (propsString.Any())
			content +=
				$"\r\n\t\tdbHandler.Execute(" +
				$"\r\n\t\t\t@\"ALTER TABLE {type.TableName} " +
				$"{string.Join("", propsString)}\"" +
				$"\r\n\t\t\t);";

		propsString.Clear();

		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
		{
			(string props, string tableName) = ScriptBuilder.BuildManyToMany(prop);

			if (!content.Contains($"CREATE TABLE {tableName}"))
				content +=
					$"\r\n\t\tdbHandler.Execute(" +
					$"\r\n\t\t\t@\"CREATE TABLE {tableName} " +
					$"{props}\"" +
					$"\r\n\t\t\t);";

			propsString.Clear();
		}

		return content;
	}

	private static string HandleManyToManyForUp(this string content, AttributeHelpers.ClassProps type, string snapshotContent)
	{
		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
		{
			string propsString;
			string tableName = ScriptBuilder.BuildManyToMany(prop).TableName;
			propsString = HandlePropertyOptions(prop, Operation.Create);

			if (!snapshotContent.Contains($"CREATE TABLE {tableName}")
				&& !content.Contains($"CREATE TABLE {tableName}"))
				content +=
					$"\r\n\t\tdbHandler.Execute(" +
					$"\r\n\t\t\t@\"CREATE TABLE {tableName} " +
					$"{propsString}\"" +
					$"\r\n\t\t\t);";
		}

		return content;
	}

	private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";


		content += $"\r\n\t\tdbHandler.Execute(\"DROP TABLE {tableName}\");";

		if (type.Properties.Any(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
		{
			foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
			{
				tableName = ScriptBuilder.BuildManyToMany(prop).TableName;

				if (content.Contains($"CREATE TABLE {tableName}"))
					content += $"\r\n\t\tdbHandler.Execute(\"DROP TABLE {tableName}\");";
			}
		}

		return content;
	}

	private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement? modelStatement)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
		{
			content += $"\r\n\t\tdbHandler.Execute(\"DROP TABLE {tableName}\");";

			if (type.Properties.Any(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
			{
				foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
				{
					tableName = ScriptBuilder.BuildManyToMany(prop).TableName;

					if (!snapshotContent.Contains($"CREATE TABLE {tableName}") 
						&& (content.Contains($"CREATE TABLE {tableName}")))
					{
						content += $"\r\n\t\tdbHandler.Execute(\"DROP TABLE {tableName}\");";
					}
				}
			}
		}
		else
			content = content.HandleEntityChanges(tableName, type, snapshotContent, modelStatement, Method.Down);

		return content;
	}

	internal static string HandlePropertyOptions(AttributeHelpers.Property prop, Operation operation)
	{
		string content = "";

		if (prop.Attributes.Select(x => x.Name).Contains("PrimaryGeneratedColumn"))
			ScriptBuilder.BuildPrimaryKey(ref content, prop);
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("OneToOne"))
		{
			string relationshipString = GetRelationship(prop) == Relationship.Optional
				? "NULL"
				: "NOT NULL, " +
				ScriptBuilder.BuildForeignKey(prop.ParentClass.TableName, prop) +
				$"REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)" +
				(GetCascadeOption(prop) ? " ON DELETE CASCADE" : "");

			content += $"{prop.ColumnName} INT UNIQUE {relationshipString}";
		}
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("ManyToOne"))
			content += $"{prop.ColumnName} INT, {(operation == Operation.Alter ? "ADD" : "")} {ScriptBuilder.BuildForeignKey(prop.ParentClass.TableName, prop).Replace("ADD", "")} REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)";
        else if (prop.Attributes.Select(x => x.Name).Single().Contains("ManyToMany"))
		{
			content = ScriptBuilder.BuildManyToMany(prop).Content;
		}
        else if (prop.Attributes.Select(x => x.Name).Contains("Column"))
		{
			ScriptBuilder.GetDataType(ref content, prop);

			//if (prop.AttributeProps.Any(x => x.Key == "Unique" && (bool)x.Value == true))
			//	content += operation == Operation.Create ? " UNIQUE" : $" ADD UNIQUE({prop.ColumnName})";

			if (prop.AttributeProps.Any(x => x.Key == "Nullable" && (bool)x.Value == false)) 
				content += " NOT NULL";

			if (prop.AttributeProps.Any(x => x.Key == "DefaultValue" && x.Value != null))
			{
				var defaultValue = prop.AttributeProps.First(x => x.Key == "DefaultValue").Value;
				string defaultValueString = defaultValue.GetType() == typeof(string) ? $"'{defaultValue}'" : defaultValue.ToString();
				content += $" DEFAULT {defaultValueString}";
			}
		}

		return content;
	}

	private static string HandleEntityChanges(this string content, string tableName, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement modelStatement, Method method)
	{
		List<string> propsString = new(); 

		foreach (var prop in type.Properties.Where(x => !x.Attributes.Any(
			y => y.FullName!.Contains("OneToMany") ||
			y.FullName!.Contains("ManyToMany"))))
		{
			if (!modelStatement.Columns.Any(x => x.PropertyName == prop.Name))
			{
				if (method == Method.Up)
					propsString.Add($"ADD {HandlePropertyOptions(prop, Operation.Alter)}");
				else
					propsString.Add($"DROP COLUMN {prop.ColumnName}");
			}

			if (modelStatement.Columns.Any(x => x.PropertyName == prop.Name && x.ColumnName != prop.ColumnName))
			{
				string currentColumnName = modelStatement.Columns.First(x => x.PropertyName == prop.Name).ColumnName;

				if (method == Method.Up)
					content += $"\r\n\t\tdbHandler.Execute(\"{ScriptBuilder.Rename(tableName, currentColumnName, prop.ColumnName)}\");";
				else
					content += $"\r\n\t\tdbHandler.Execute(\"{ScriptBuilder.Rename(tableName, prop.ColumnName, currentColumnName)}\");";
			}

			if (modelStatement.Columns.Any(x => x.PropertyName == prop.Name && x.PropertyOptions != HandlePropertyOptions(prop, Operation.Create).RemoveFormatting().Substring(prop.ColumnName.Length + 1)))
			{
				if (method == Method.Up)
					propsString.Add($"ALTER COLUMN {HandlePropertyOptions(prop, Operation.Alter)}");
				else
					propsString.Add($"ALTER COLUMN {modelStatement.Columns.First(x => x.PropertyName == prop.Name).PropertyOptions}");
			}
		}

		foreach (var column in modelStatement.Columns)
		{
			if (!type.Properties.Select(x => x.Name).Contains(column.PropertyName))
			{
				if (method == Method.Up)
					propsString.Add($"DROP COLUMN {column.ColumnName}");
				else
					propsString.Add($"ADD {column.ColumnName} {column.PropertyOptions}");
			}
		}

		if (propsString.Count() > 0)
			content += $"\r\n\t\tdbHandler.Execute(\"ALTER TABLE {tableName} {string.Join(", ", propsString)}\");";

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
