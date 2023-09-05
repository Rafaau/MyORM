using ORM.Common;

namespace CLI.Methods;

internal static class SnapshotFactory
{
	public static string ProduceShapshotContent(List<AttributeHelpers.ClassProps> types, string nameSpace)
	{
		string content = $"using ORM.Abstract;\r\nusing ORM.Attributes;\r\nusing ORM.Models;\r\n\r\nnamespace {nameSpace}.Migrations;\r\n\r\n[Snapshot]\r\npublic partial class ModelSnapshot : AbstractSnapshot\r\n{{\r\n\tpublic override string GetMetadata()\r\n\t{{\r\n\t\treturn \"\";\r\n\t}}\r\n\tpublic override void CreateDBFromSnapshot(Schema schema)\r\n\t{{";

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
        string model = $"new ModelStatement(\"{type.ClassName}\", \"{type.ClassName}\", new List<ColumnStatement>()\r\n\t\t{{";

        foreach (var prop in type.Properties)
        {
            bool isRelational = prop.Attributes.Any(attribute => attribute.FullName.Contains("OneToOne"));
            int index = isRelational ? 2 : 1;
            model += $"\r\n\t\t\tnew ColumnStatement(\"{prop.Name}\", \"{MigrationFactory.HandlePropertyOptions(prop, MigrationFactory.Operation.Create).Substring(prop.Name.Length + index)}\"),";
        }

        model += "\r\n\t\t})";

        content += $"\r\n\t\tmodels.Add({model});";

        return content;
    }
}
