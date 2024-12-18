﻿using MyORM.Methods;
using MyORM.Models;
using MyORM.DBMS;
using MyORM.CLI.Enums;

namespace MyORM.CLI.Methods;

/// <summary>
/// Factory class to produce migration content.
/// </summary>
internal static class MigrationFactory
{
    /// <summary>
    /// Produces migration content.
    /// </summary>
    /// <param name="types">List of entity types</param>
    /// <param name="nameSpace">Namespace of the migration</param>
    /// <param name="migrationName">Name of the migration</param>
    /// <param name="snapshotContent">Content of the snapshot</param>
    /// <param name="databaseManagementSystem">Database management system</param>
    /// <returns>Returns the migration content</returns>
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

    /// <summary>
    /// Produces the script for the single entity.
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="type">Entity type</param>
    /// <param name="snapshotContent">Snapshot content</param>
    /// <param name="modelStatement">Model statement of the current entity</param>
    /// <returns>Returns the content with the current entity script</returns>
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
			propsString += ScriptBuilder.HandlePropertyOptions(prop, Operation.Create);
			if (index != properties.Count())
				propsString += ", ";
			index++;
		}

		if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
			content += $"\r\n\t\tdbHandler.Execute(@\"CREATE TABLE {tableName} ({propsString})\");";
		else
			content = content.HandleEntityChanges(tableName, type, snapshotContent, modelStatement, Method.Up);

		return content;
	}

    /// <summary>
    /// Produces the script for the single entity.
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="type">Entity type</param>
    /// <returns>Returns the content with the current entity script</returns>
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
			propsString.Add($"\r\n\t\t\t\t{ScriptBuilder.HandlePropertyOptions(prop, Operation.Create)}");

		content += 
			$"\r\n\t\tdbHandler.Execute(" +
			$"\r\n\t\t\t@\"CREATE TABLE {type.TableName} (" +
			$"{string.Join(", ", propsString)}" +
			$"\r\n\t\t\t)\"" +
			$"\r\n\t\t);";

		return content;
	}

    /// <summary>
    /// Produces the script for the single entity relation properties.
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="type">Entity type</param>
    /// <returns>Returns the content with the current entity relation properties script</returns>
    internal static string HandleEntityRelationPropsForUp(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");

		List<string> propsString = new();

		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => 
			x.FullName!.Contains("OneToOne") ||
			x.FullName!.Contains("ManyToOne")))
		)
		{
			string relationshipString = prop.HasRelationship(Relationship.Optional)
				? "NULL" 
				: "NOT NULL, " +
				ScriptBuilder.BuildForeignKey(type.TableName, prop) +
				$"REFERENCES {prop.RelatedClass.TableName}({prop.RelatedClass.PrimaryKeyColumnName})" +
				(prop.HasCascadeOption() ? " ON DELETE CASCADE" : "");

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

    /// <summary>
    /// Produces the script for the many-to-many relationship for the single entity.
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="type">Entity type</param>
    /// <param name="snapshotContent">Snapshot content</param>
    /// <returns>Returns the content with the current entity many-to-many relationship script</returns>
    private static string HandleManyToManyForUp(this string content, AttributeHelpers.ClassProps type, string snapshotContent)
	{
		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
		{
			string propsString;
			string tableName = ScriptBuilder.BuildManyToMany(prop).TableName;
			propsString = ScriptBuilder.HandlePropertyOptions(prop, Operation.Create);

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

    /// <summary>
    /// Produces the script for the single entity (down method).
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="type">Entity type</param>
    /// <returns>Returns the content with the current entity script</returns>
    private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";


		content += $"\r\n\t\tdbHandler.Execute(@\"DROP TABLE {tableName}\");";

		if (type.Properties.Any(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
		{
			foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
			{
				tableName = ScriptBuilder.BuildManyToMany(prop).TableName;

				if (content.Contains($"CREATE TABLE {tableName}"))
					content += $"\r\n\t\tdbHandler.Execute(@\"DROP TABLE {tableName}\");";
			}
		}

		return content;
	}

    /// <summary>
    /// Produces the script for the single entity (down method).
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="type">Entity type</param>
    /// <param name="snapshotContent">Snapshot content</param>
    /// <param name="modelStatement">Model statement of the current entity</param>
    /// <returns>Returns the content with the current entity script</returns>
    private static string HandleEntityPropsForDown(this string content, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement? modelStatement)
	{
		var name = type.AttributeProps.Where(x => x.Key == "Name");
		string tableName = name != null ? name.First().Value.ToString() : type.ClassName + "s";

		if (!snapshotContent.Contains($"CREATE TABLE {tableName}"))
		{
			content += $"\r\n\t\tdbHandler.Execute(@\"DROP TABLE {tableName}\");";

			if (type.Properties.Any(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
			{
				foreach (var prop in type.Properties.Where(x => x.Attributes.Any(x => x.FullName!.Contains("ManyToMany"))))
				{
					tableName = ScriptBuilder.BuildManyToMany(prop).TableName;

					if (!snapshotContent.Contains($"CREATE TABLE {tableName}") 
						&& (content.Contains($"CREATE TABLE {tableName}")))
					{
						content += $"\r\n\t\tdbHandler.Execute(@\"DROP TABLE {tableName}\");";
					}
				}
			}
		}
		else
			content = content.HandleEntityChanges(tableName, type, snapshotContent, modelStatement, Method.Down);

		return content;
	}

    /// <summary>
    /// Searches for changes in the entity by comparing the snapshot and the current entity state.
    /// </summary>
    /// <param name="content">Actual content</param>
    /// <param name="tableName">Table name of the entity</param>
    /// <param name="type">Entity type</param>
    /// <param name="snapshotContent">Snapshot content</param>
    /// <param name="modelStatement">Model statement of the current entity</param>
    /// <param name="method">Migration method</param>
    /// <returns>Returns the content with the changes in the entity</returns>
    private static string HandleEntityChanges(this string content, string tableName, AttributeHelpers.ClassProps type, string snapshotContent, ModelStatement modelStatement, Method method)
	{
		List<string> propsString = new(); 

		foreach (var prop in type.Properties.Where(x => !x.Attributes.Any(
			y => y.FullName!.Contains("OneToMany") ||
			y.FullName!.Contains("ManyToMany"))))
		{
			if (!modelStatement.Columns.ContainsProperty(prop))
			{
				if (method == Method.Up)
					propsString.Add($"ADD {ScriptBuilder.HandlePropertyOptions(prop, Operation.Alter)}");
				else
					propsString.Add($"DROP COLUMN {prop.ColumnName}");
			}

			if (modelStatement.Columns.ColumnNameHasChanged(prop))
			{
				string currentColumnName = modelStatement.Columns.First(x => x.PropertyName == prop.Name).ColumnName;

				if (method == Method.Up)
					content += $"\r\n\t\tdbHandler.Execute(@\"{ScriptBuilder.Rename(tableName, currentColumnName, prop.ColumnName)}\");";
				else
					content += $"\r\n\t\tdbHandler.Execute(@\"{ScriptBuilder.Rename(tableName, prop.ColumnName, currentColumnName)}\");";
			}

			if (modelStatement.Columns.PropertyOptionsHaveChanged(prop))
			{
				if (method == Method.Up)
					propsString.Add($"ALTER COLUMN {ScriptBuilder.HandlePropertyOptions(prop, Operation.Alter)}");
				else
					propsString.Add($"ALTER COLUMN {modelStatement.Columns.First(x => x.PropertyName == prop.Name).PropertyOptions}");
			}

			if (modelStatement.Columns.ColumnBecameUnique(prop))
			{
				if (method == Method.Up)
					content += $"\r\n\t\tdbHandler.Execute(@\"ALTER TABLE {tableName} ADD CONSTRAINT {prop.ColumnName}_unique UNIQUE ({prop.ColumnName})\");";
				else
					content += $"\r\n\t\tdbHandler.Execute(@\"ALTER TABLE {tableName} DROP CONSTRAINT {prop.ColumnName}_unique\");";
			}

			if (modelStatement.Columns.ColumnLostUnique(prop))
			{
				if (method == Method.Up)
					content += $"\r\n\t\tdbHandler.Execute(@\"ALTER TABLE {tableName} DROP CONSTRAINT {prop.ColumnName}_unique\");";
				else
					content += $"\r\n\t\tdbHandler.Execute(@\"ALTER TABLE {tableName} ADD CONSTRAINT {prop.ColumnName}_unique UNIQUE ({prop.ColumnName})\");";
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
			content += $"\r\n\t\tdbHandler.Execute(@\"ALTER TABLE {tableName} {string.Join(", ", propsString)}\");";

		return content;
	}
}
