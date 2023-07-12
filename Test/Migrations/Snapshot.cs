using ORM.Abstract;
using ORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class Snapshot : AbstractSnapshot
{
	public override string GetMetadata()
	{
		return "";
	}
	public override void CreateDBFromSnapshot(Schema schema)
	{
		schema.Execute("CREATE TABLE users (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255))");
	}
}