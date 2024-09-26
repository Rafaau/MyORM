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
		dbHandler.Execute("CREATE TABLE users (Id INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id), Name VARCHAR(255), Email VARCHAR(255))");
		dbHandler.Execute("ALTER TABLE accounts ADD UserId INT UNIQUE NOT NULL, ADD FOREIGN KEY (UserId) REFERENCES accounts(Id)");
		dbHandler.Execute("ALTER TABLE users ADD AccountId INT UNIQUE NULL, ADD FOREIGN KEY (AccountId) REFERENCES users(Id)");
	}
	public override List<ModelStatement> GetModelsStatements()
	{
		List<ModelStatement> models = new();

		models.Add(new ModelStatement("Account", "Account", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id)"),
			new ColumnStatement("Nickname", "VARCHAR(255)"),
			new ColumnStatement("User", " INT UNIQUE,  FOREIGN KEY (UserId) REFERENCES users(Id)"),
		}));
		models.Add(new ModelStatement("User", "User", new List<ColumnStatement>()
		{
			new ColumnStatement("Id", "INT AUTO_INCREMENT NOT NULL, PRIMARY KEY (Id)"),
			new ColumnStatement("Name", "VARCHAR(255)"),
			new ColumnStatement("Email", "VARCHAR(255)"),
			new ColumnStatement("Account", " INT UNIQUE,  FOREIGN KEY (AccountId) REFERENCES accounts(Id)"),
		}));

		return models;
	}
}