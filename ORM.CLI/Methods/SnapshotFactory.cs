﻿using MyORM;
using MyORM.DBMS;
using MyORM.Methods;

namespace MyORM.CLI.Methods;

/// <summary>
/// Factory class for creating a snapshot file.
/// </summary>
internal static class SnapshotFactory
{
    /// <summary>
    /// Produces the content of the snapshot file.
    /// </summary>
    /// <param name="types">List of entity types</param>
    /// <param name="nameSpace">Namespace of the snapshot</param>
    /// <param name="databaseManagementSystem">Database management system</param>
    /// <returns>Returns the content of the snapshot file</returns>
    public static string ProduceShapshotContent(List<AttributeHelpers.ClassProps> types, string nameSpace, Database databaseManagementSystem)
	{
		ScriptBuilder.Database = databaseManagementSystem;

		string content = 
			$"using MyORM;\r\n" + 
			$"using MyORM.Models;\r\n\r\n" + 
			$"namespace {nameSpace}.Migrations;\r\n\r\n" + 
			$"[Snapshot]\r\n" +
			$"public partial class ModelSnapshot : AbstractSnapshot\r\n{{\r\n\t" +
			$"public override string GetMetadata()\r\n\t{{\r\n\t\t" +
			$"return\r\n\t\t\t" +
			$"@\"\r\n\t\t\t\t" +
			$"targetDBMS: {databaseManagementSystem.ToString()}\r\n\t\t\t" +
			$"\";\r\n\t}}\r\n\t" +
			$"public override void CreateDBFromSnapshot(DbHandler dbHandler)\r\n\t{{";

		foreach (var type in types)
		{
			content = content.HandleEntityPropsForUp(type);
		}

		foreach (var type in types)
		{
			content = content.HandleEntityRelationPropsForUp(type);
		}

		content += "\r\n\t}\r\n\tpublic override List<ModelStatement> GetModelsStatements()\r\n\t{";
		content += "\r\n\t\tList<ModelStatement> models = new();\r\n";


		foreach (var type in types)
		{
			content = content.GenerateModelStatement(type);
		}

		content += "\r\n\r\n\t\treturn models;";
		content += "\r\n\t}\r\n}";

		return content;
	}

    /// <summary>
    /// Generates the single model statement to the snapshot file.
    /// </summary>
    /// <param name="content">Actual content of the snapshot file</param>
    /// <param name="type">Entity type</param>
    /// <returns>Returns the content of the snapshot file</returns>
    private static string GenerateModelStatement(this string content, AttributeHelpers.ClassProps type)
	{
		string model = $"new ModelStatement(\"{type.ClassName}\", \"{type.TableName}\", new List<ColumnStatement>()\r\n\t\t{{";

		foreach (var prop in type.Properties.Where(x => !x.Attributes.Any(y =>
			y.FullName.Contains("OneToMany") ||
			y.FullName.Contains("ManyToMany"))))
		{
			model += 
				$"\r\n\t\t\tnew ColumnStatement(\"{prop.Name}\", \"{prop.ColumnName}\", \"{ScriptBuilder
					.HandlePropertyOptions(prop, Operation.Create)
					.RemoveFormatting()
					.Substring(prop.ColumnName.Length + 1)}\"),";
		}

		model += "\r\n\t\t}";

		if (type.Properties.Any(x => x.Attributes.Any(y => y.FullName.Contains("ManyToMany"))))
			model += ",\r\n\t\tnew List<RelationStatement>()\r\n\t\t{";

		foreach (var prop in type.Properties.Where(x => x.Attributes.Any(y => y.FullName.Contains("ManyToMany"))))
		{
			string tableName = ScriptBuilder.BuildManyToMany(prop).TableName;
			
			string currentModelName = prop.ParentClass.ClassName;
			string relationModelName = prop.Type.GetGenericArguments()[0].Name;
			string columns = currentModelName == relationModelName
				? $"\"{currentModelName}Id\", \"{relationModelName}1Id\""
				: $"\"{currentModelName}Id\", \"{relationModelName}Id\"";
			model += $"\r\n\t\t\tnew RelationStatement(\"{prop.Name}\", \"{tableName}\", {columns}),";
		}

		if (model.Contains("new RelationStatement"))
			model += "\r\n\t\t}";

		model += ")";

		content += $"\r\n\t\tmodels.Add({model});";

		return content;
	}
}
