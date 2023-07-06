namespace CLI.Methods;

internal static class SnapshotFactory
{
	public static string ProduceShapshotContent(List<AttributeHelpers.ClassProps> types, string nameSpace)
	{
		string content = $"using ORM.Abstract;\r\nusing ORM.Attributes;\r\n\r\nnamespace {nameSpace}.Migrations;\r\n\r\n[Migration]\r\npublic partial class Snapshot : AbstractSnapshot\r\n{{\r\n\tpublic override string GetMetadata()\r\n\t{{\r\n\t\treturn \"\";\r\n\t}}\r\n\tpublic override void CreateDBFromSnapshot(Schema schema)\r\n\t{{";

		foreach (var type in types)
		{
			content = content.HandleEntityPropsForUp(type);
		}

		content += "\r\n\t}\r\n}";

		return content;
	}
}
