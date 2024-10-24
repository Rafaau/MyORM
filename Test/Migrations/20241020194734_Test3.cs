using MyORM.Abstract;
using MyORM.Attributes;

namespace Test.Migrations;

[Migration]
public partial class M20241020194734_Test3 : AbstractMigration
{
	public override string GetDescription()
	{
		return "";
	}
	public override void Up(DbHandler dbHandler)
	{
		dbHandler.Execute("CREATE TABLE tags (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255))");
		dbHandler.Execute("CREATE TABLE postTagsTag (PostId INT NOT NULL, TagId INT NOT NULL, CONSTRAINT PK_postTagsTag PRIMARY KEY (PostId, TagId), CONSTRAINT FK_postTagsTag_Posts_PostId FOREIGN KEY (PostId) REFERENCES posts(Id) ON DELETE CASCADE, CONSTRAINT FK_postTagsTag_Tags_TagId FOREIGN KEY (TagId) REFERENCES tags(Id) ON DELETE CASCADE)");
	}
	public override void Down(DbHandler dbHandler)
	{
		dbHandler.Execute("DROP TABLE tags");
		dbHandler.Execute("DROP TABLE postPostsTag");
	}
}
