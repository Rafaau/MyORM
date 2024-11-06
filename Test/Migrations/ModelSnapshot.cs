using MyORM;
using MyORM.Models;

namespace Test.Migrations;

[Snapshot]
public partial class ModelSnapshot : AbstractSnapshot
{
	public override string GetMetadata()
	{
		return
			@"
				targetDBMS: MicrosoftSQLServer
			";
	}
	public override void CreateDBFromSnapshot(DbHandler dbHandler)
	{
		dbHandler.Execute("CREATE TABLE accounts (Id INT IDENTITY(1,1) PRIMARY KEY, Nickname NVARCHAR(255))");
		dbHandler.Execute("CREATE TABLE posts (Id INT IDENTITY(1,1) PRIMARY KEY, SendDate DATETIME, Content NVARCHAR(255))");
		dbHandler.Execute("CREATE TABLE tags (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255))");
		dbHandler.Execute("CREATE TABLE users (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(255), Email NVARCHAR(255))");
		dbHandler.Execute("ALTER TABLE accounts ADD UserId INT UNIQUE NOT NULL, CONSTRAINT FK_accounts_users_UserId FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE");
		dbHandler.Execute("ALTER TABLE posts ADD AccountId INT NOT NULL, CONSTRAINT FK_posts_accounts_AccountId FOREIGN KEY (AccountId) REFERENCES accounts(Id)");
		dbHandler.Execute("CREATE TABLE postPostsTag (PostId INT NOT NULL, TagId INT NOT NULL, PRIMARY KEY (PostId, TagId), CONSTRAINT FK_postPostsTag_Posts_PostId FOREIGN KEY (PostId) REFERENCES posts(Id) ON DELETE CASCADE, CONSTRAINT FK_postPostsTag_Tags_TagId FOREIGN KEY (TagId) REFERENCES tags(Id) ON DELETE CASCADE)");
		dbHandler.Execute("ALTER TABLE users ADD AccountId INT UNIQUE NULL");
		dbHandler.Execute("CREATE TABLE userFriendsUser (UserId INT NOT NULL, User1Id INT NOT NULL, PRIMARY KEY (UserId, User1Id), CONSTRAINT FK_userFriendsUser_Users_UserId FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE NO ACTION, CONSTRAINT FK_userFriendsUser_Users_User1Id FOREIGN KEY (User1Id) REFERENCES users(Id) ON DELETE NO ACTION)");
	}
	public override List<ModelStatement> GetModelsStatements()
	{
		List<ModelStatement> models = new();

		models.Add(new ModelStatement("Account", "accounts", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "Id", "Id INT IDENTITY(1,1) PRIMARY KEY"),
			new ColumnStatement("Nickname", "Nickname", "Nickname NVARCHAR(255)"),
			new ColumnStatement("User", "UserId", "UserId INT UNIQUE NOT NULL, CONSTRAINT FK_accounts_users_UserId FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE"),
		}));
		models.Add(new ModelStatement("Post", "posts", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "Id", "Id INT IDENTITY(1,1) PRIMARY KEY"),
			new ColumnStatement("SendDate", "SendDate", "SendDate DATETIME"),
			new ColumnStatement("Content", "Content", "Content NVARCHAR(255)"),
			new ColumnStatement("Account", "AccountId", "AccountId INT,  CONSTRAINT FK_posts_accounts_AccountId FOREIGN KEY (AccountId)  REFERENCES accounts(Id)"),
		},
		new List<RelationStatement>()
		{
			new RelationStatement("Tags", "postPostsTag", "PostId", "TagId"),
		}));
		models.Add(new ModelStatement("Tag", "tags", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "Id", "Id INT IDENTITY(1,1) PRIMARY KEY"),
			new ColumnStatement("Name", "Name", "Name NVARCHAR(255)"),
		},
		new List<RelationStatement>()
		{
			new RelationStatement("Posts", "postPostsTag", "TagId", "PostId"),
		}));
		models.Add(new ModelStatement("User", "users", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "Id", "Id INT IDENTITY(1,1) PRIMARY KEY"),
			new ColumnStatement("Name", "Name", "Name NVARCHAR(255)"),
			new ColumnStatement("Email", "Email", "Email NVARCHAR(255)"),
			new ColumnStatement("Account", "AccountId", "AccountId INT UNIQUE NULL"),
		},
		new List<RelationStatement>()
		{
			new RelationStatement("Friends", "userFriendsUser", "UserId", "User1Id"),
		}));

		return models;
	}
}