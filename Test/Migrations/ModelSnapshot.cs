using MyORM.Abstract;
using MyORM.Attributes;
using MyORM.Models;

namespace Test.Migrations;

[Snapshot]
public partial class ModelSnapshot : AbstractSnapshot
{
	public override string GetMetadata()
	{
		return "";
	}
	public override void CreateDBFromSnapshot(DbHandler dbHandler)
	{
		dbHandler.Execute("CREATE TABLE accounts (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Nickname VARCHAR(255))");
		dbHandler.Execute("CREATE TABLE posts (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), SendDate DATETIME, Content VARCHAR(255))");
		dbHandler.Execute("CREATE TABLE users (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255), Email VARCHAR(255))");
		dbHandler.Execute("ALTER TABLE accounts ADD UserId INT UNIQUE NOT NULL, ADD FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE");
		dbHandler.Execute("ALTER TABLE posts ADD AccountId INT NOT NULL, ADD FOREIGN KEY (AccountId) REFERENCES accounts(Id)");
		dbHandler.Execute("ALTER TABLE users ADD AccountId INT UNIQUE NULL");
	}
	public override List<ModelStatement> GetModelsStatements()
	{
		List<ModelStatement> models = new();

		models.Add(new ModelStatement("Account", "accounts", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id)"),
			new ColumnStatement("Nickname", "VARCHAR(255)"),
			new ColumnStatement("User", "INT UNIQUE NOT NULL, ADD FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE"),
		}));
		models.Add(new ModelStatement("Post", "posts", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id)"),
			new ColumnStatement("SendDate", "DATETIME"),
			new ColumnStatement("Content", "VARCHAR(255)"),
			new ColumnStatement("Account", "INT,  FOREIGN KEY (Account) REFERENCES accounts(Id)"),
		}));
		models.Add(new ModelStatement("User", "users", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id)"),
			new ColumnStatement("Name", "VARCHAR(255)"),
			new ColumnStatement("Email", "VARCHAR(255)"),
			new ColumnStatement("Account", "INT UNIQUE NULL"),
		}));

		return models;
	}
}
