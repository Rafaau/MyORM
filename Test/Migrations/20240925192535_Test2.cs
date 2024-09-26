using MyORM.Abstract;
using MyORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20240925192535_Test2 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(DbHandler dbHandler)
	{
	}
	public override void Down(DbHandler dbHandler)
	{
	}
}
