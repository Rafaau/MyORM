using ORM.Abstract;
using ORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20230804125937_test3 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(Schema schema)
	{
		schema.Execute("CREATE TABLE users (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255), Email VARCHAR(255))");
	}
	public override void Down(Schema schema)
	{
		schema.Execute("DROP TABLE users");
	}
}
