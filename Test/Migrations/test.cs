using ORM.Abstract;
using ORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class test : AbstractMigration
{
	public override string GetDescription()
	{
		return "test";
	}

	public override void Up(Schema schema)
	{
		schema.Execute("CREATE TABLE test (id INT NOT NULL AUTO_INCREMENT, PRIMARY KEY (id))");
	}

	public override void Down(Schema schema)
	{
		schema.Execute("DROP TABLE test");
	}
}
