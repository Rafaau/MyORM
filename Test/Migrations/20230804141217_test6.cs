using ORM.Abstract;
using ORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20230804141217_test6 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(Schema schema)
	{
		schema.Execute("ALTER TABLE users ADD Email VARCHAR(255)");
	}
	public override void Down(Schema schema)
	{
		schema.Execute("ALTER TABLE users DROP COLUMN Email");
	}
}
