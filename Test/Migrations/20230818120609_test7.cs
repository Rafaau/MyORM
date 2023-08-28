using ORM.Abstract;
using ORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20230818120609_test7 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(Schema schema)
	{
		schema.Execute("CREATE TABLE accounts (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Nickname VARCHAR(255), UserId INT UNIQUE,  FOREIGN KEY (UserId) REFERENCES users(Id))");
		schema.Execute("ALTER TABLE users ADD AccountId INT UNIQUE, ADD FOREIGN KEY (AccountId) REFERENCES accounts(Id)");
	}
	public override void Down(Schema schema)
	{
		schema.Execute("DROP TABLE accounts");
		schema.Execute("ALTER TABLE users DROP COLUMN AccountId");
	}
}
