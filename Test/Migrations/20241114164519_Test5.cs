using MyORM;
namespace Test.Migrations;

[Migration]
public partial class M20241114164519_Test1 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(DbHandler dbHandler)
	{
		dbHandler.Execute("ALTER TABLE accounts ADD CONSTRAINT Nickname_unique UNIQUE (Nickname)");
	}
	public override void Down(DbHandler dbHandler)
	{
		dbHandler.Execute("ALTER TABLE accounts DROP CONSTRAINT Nickname_unique");
	}
}
