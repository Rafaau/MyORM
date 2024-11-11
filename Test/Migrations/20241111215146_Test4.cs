using MyORM;
namespace Test.Migrations;

[Migration]
public partial class M20241111215146_Test4 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(DbHandler dbHandler)
	{
		dbHandler.Execute("EXEC sp_rename 'users.Id', 'UUID', 'COLUMN'");
		dbHandler.Execute("ALTER TABLE users ALTER COLUMN Name NVARCHAR(255) NOT NULL, ALTER COLUMN Email NVARCHAR(255) DEFAULT 'default@gmail.com'");
	}
	public override void Down(DbHandler dbHandler)
	{
		dbHandler.Execute("EXEC sp_rename 'users.UUID', 'Id', 'COLUMN'");
		dbHandler.Execute("ALTER TABLE users ALTER COLUMN NVARCHAR(255), ALTER COLUMN NVARCHAR(255)");
	}
}
