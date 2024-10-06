using MyORM.Common.Methods;

namespace MyORM.CLI.Methods;

internal static class SnapshotFactory
{
	public static string ProduceShapshotContent(List<AttributeHelpers.ClassProps> types, string nameSpace)
	{
		string content = 
			$"using MyORM.Abstract;\r\n" + 
			$"using MyORM.Attributes;\r\n" + 
			$"using MyORM.Models;\r\n\r\n" + 
			$"namespace {nameSpace}.Migrations;\r\n\r\n" + 
			$"[Snapshot]\r\n" +
			$"public partial class ModelSnapshot : AbstractSnapshot\r\n{{\r\n\t" +
			$"public override string GetMetadata()\r\n\t{{\r\n\t\t" +
			$"return \"\";\r\n\t}}\r\n\t" +
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

	private static string GenerateModelStatement(this string content, AttributeHelpers.ClassProps type)
	{
		string model = $"new ModelStatement(\"{type.ClassName}\", \"{type.TableName}\", new List<ColumnStatement>()\r\n\t\t{{";

		foreach (var prop in type.Properties.Where(x => !x.Attributes.Any(y => y.FullName.Contains("OneToMany"))))
		{
			bool isRelational = prop.Attributes.Any(attribute =>
				attribute.FullName!.Contains("OneToOne"));

			int index = isRelational ? 3 : 1;
			model += $"\r\n\t\t\tnew ColumnStatement(\"{prop.Name}\", \"{prop.ColumnName}\", \"{MigrationFactory.HandlePropertyOptions(prop, MigrationFactory.Operation.Create).Substring(prop.Name.Length + index)}\"),";
		}

		model += "\r\n\t\t})";

		content += $"\r\n\t\tmodels.Add({model});";

		return content;
	}
}
