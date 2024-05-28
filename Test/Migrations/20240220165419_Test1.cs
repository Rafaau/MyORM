using ORM.Abstract;
using ORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20240220165419_Test1 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(DbHandler dbHandler)
	{
		dbHandler.Execute("CREATE TABLE accounts (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Nickname VARCHAR(255))");
		dbHandler.Execute("CREATE TABLE users (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255), Email VARCHAR(255))");
		dbHandler.Execute("ALTER TABLE accounts ADD UserId INT UNIQUE NOT NULL, ADD FOREIGN KEY (UserId) REFERENCES accounts(Id)");
		dbHandler.Execute("ALTER TABLE users ADD AccountId INT UNIQUE NULL, ADD FOREIGN KEY (AccountId) REFERENCES users(Id)");
	}
	public override void Down(DbHandler dbHandler)
	{
		dbHandler.Execute("DROP TABLE accounts");
		dbHandler.Execute("DROP TABLE users");
	}
}
