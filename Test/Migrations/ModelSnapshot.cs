using ORM.Abstract;
using ORM.Attributes;
using ORM.Models;

namespace Test.Migrations;

[Snapshot]
public partial class ModelSnapshot : AbstractSnapshot
{
	public override string GetMetadata()
	{
		return "";
	}
	public override void CreateDBFromSnapshot(Schema schema)
	{
		schema.Execute("CREATE TABLE users (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255), Email VARCHAR(255))");
	}
	public override List<ModelStatement> GetModelsStatements()
	{
		List<ModelStatement> models = new();

		models.Add(new ModelStatement("User", "User", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id)"),
			new ColumnStatement("Name", "VARCHAR(255)"),
			new ColumnStatement("Email", "VARCHAR(255)"),
		}));

		return models;
	}
}