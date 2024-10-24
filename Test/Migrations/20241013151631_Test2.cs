using MyORM.Abstract;
using MyORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20241013151631_Test2 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(DbHandler dbHandler)
	{
		dbHandler.Execute("CREATE TABLE userFriendsUser (UserId INT NOT NULL, User1Id INT NOT NULL, CONSTRAINT PK_userFriendsUser PRIMARY KEY (UserId, User1Id), CONSTRAINT FK_userFriendsUser_Users_UserId FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE, CONSTRAINT FK_userFriendsUser_User1s_User1Id FOREIGN KEY (User1Id) REFERENCES users(Id) ON DELETE CASCADE)");
	}
	public override void Down(DbHandler dbHandler)
	{
	}
}
