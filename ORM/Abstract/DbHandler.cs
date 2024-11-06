using MyORM.DBMS;
using MyORM.Methods;
using System.Data;

namespace MyORM;

/// <summary>
/// Class that handles the database connection and operations.
/// </summary>
public class DbHandler
{
	public AccessLayer AccessLayer { get; set; }
	private DatabaseManager databaseManager { get; set; }

	/// <summary>
	/// Constructor that initializes the <see cref="DatabaseManager"/> instance.
	/// </summary>
	/// <param name="accessLayer"><c>AccessLayer</c> instance.</param>
	public DbHandler(AccessLayer accessLayer)
	{
		AccessLayer = accessLayer;
		databaseManager = new DatabaseManager(accessLayer);
	}

	public void OpenConnection() => databaseManager.OpenConnection();

	public void CloseConnection() => databaseManager.CloseConnection();

	public void BeginTransaction()
	{
		databaseManager.BeginTransaction();
	}

	public void CommitTransaction()
	{
		databaseManager.CommitTransaction();
	}

	public void RollbackTransaction()
	{
		databaseManager.RollbackTransaction();
	}

	public int Execute(string sqlCommandText)
	{
		Console.WriteLine(sqlCommandText);
		return databaseManager.ExecuteNonQuery(sqlCommandText);
	}

	public void ExecuteWithTransaction(string sqlCommandText)
	{
		BeginTransaction();
		Execute(sqlCommandText);
		CommitTransaction();
	}

	public DataTable Query(string sqlCommandText)
	{
		return databaseManager.ExecuteQuery(sqlCommandText);
	}

	public bool CheckIfTableExists(string tableName)
	{
		return databaseManager.CheckIfTableExists(tableName);
	}

	public bool CheckTheLastRecord(string tableName, string columnName, string value)
	{
		return databaseManager.CheckTheLastRecord(tableName, columnName, value);
	}

	public void InsertMigrationRecord(string migrationName, bool revert = false)
	{
		Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationName}{(revert ? "_revert" : "")}', {ScriptBuilder.DateTimeNow})");
	}

	public void CreateMigrationHistoryTable()
	{
		Execute($"CREATE TABLE _MyORMMigrationsHistory ({ScriptBuilder.BuildPrimaryKey("Id")}, MigrationName VARCHAR(255) NOT NULL, Date {ScriptBuilder.DateTimeType} NOT NULL)");
	}
}
