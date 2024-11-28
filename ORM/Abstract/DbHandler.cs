using MyORM.DBMS;
using MyORM.Methods;
using System.Data;

namespace MyORM;

/// <summary>
/// Class that handles the database connection and operations.
/// </summary>
public class DbHandler
{
    /// <summary>
    /// Gets or sets the <see cref="AccessLayer"/> instance.
    /// </summary>
    public AccessLayer AccessLayer { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DatabaseManager"/> instance.
    /// </summary>
    private DatabaseManager databaseManager { get; set; }

	/// <summary>
	/// Constructor that initializes the <see cref="DatabaseManager"/> instance.
	/// </summary>
	/// <param name="accessLayer">Access layer instance</param>
	public DbHandler(AccessLayer accessLayer)
	{
		AccessLayer = accessLayer;
		databaseManager = new DatabaseManager(accessLayer);
	}

    /// <summary>
    /// Opens the connection to the database.
    /// </summary>
    public void OpenConnection() => databaseManager.OpenConnection();

    /// <summary>
    /// Closes the connection to the database.
    /// </summary>
    public void CloseConnection() => databaseManager.CloseConnection();

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    public void BeginTransaction()
	{
		databaseManager.BeginTransaction();
	}

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    public void CommitTransaction()
	{
		databaseManager.CommitTransaction();
	}

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    public void RollbackTransaction()
	{
		databaseManager.RollbackTransaction();
	}

    /// <summary>
    /// Executes a SQL command.
    /// </summary>
    /// <param name="sqlCommandText">Command text</param>
    /// <returns>Returns the number of rows affected</returns>
    public int Execute(string sqlCommandText)
	{
		Console.WriteLine(sqlCommandText);
		return databaseManager.ExecuteNonQuery(sqlCommandText);
	}

    /// <summary>
    /// Executes a SQL command with a transaction.
    /// </summary>
    /// <param name="sqlCommandText">Command text</param>
    public void ExecuteWithTransaction(string sqlCommandText)
	{
		BeginTransaction();
		Execute(sqlCommandText);
		CommitTransaction();
	}

    /// <summary>
    /// Executes a SQL query.
    /// </summary>
    /// <param name="sqlCommandText">Command text</param>
    /// <returns>Returns the result of the query</returns>
    public DataTable Query(string sqlCommandText)
	{
		return databaseManager.ExecuteQuery(sqlCommandText);
	}

    /// <summary>
    /// Checks if a table exists.
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <returns>Returns true if the table exists, false otherwise</returns>
    public bool CheckIfTableExists(string tableName)
	{
		return databaseManager.CheckIfTableExists(tableName);
	}

    /// <summary>
    /// Checks if a column exists in a table.
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <param name="columnName">Column name</param>
    /// <param name="value">Value to check</param>
    /// <returns>Returns true if the column exists, false otherwise</returns>
	public bool CheckTheLastRecord(string tableName, string columnName, string value)
	{
		return databaseManager.CheckTheLastRecord(tableName, columnName, value);
	}

    /// <summary>
    /// Inserts a migration record.
    /// </summary>
    /// <param name="migrationName">Name of the migration</param>
    /// <param name="revert">True if the migration is a revert, false otherwise</param>
	public void InsertMigrationRecord(string migrationName, bool revert = false)
	{
		Execute($"INSERT INTO _MyORMMigrationsHistory (MigrationName, Date) VALUES ('{migrationName}{(revert ? "_revert" : "")}', {ScriptBuilder.DateTimeNow})");
	}

    /// <summary>
    /// Deletes a migration record.
    /// </summary>
	public void CreateMigrationHistoryTable()
	{
		Execute($"CREATE TABLE _MyORMMigrationsHistory ({ScriptBuilder.BuildPrimaryKey("Id")}, MigrationName VARCHAR(255) NOT NULL, Date {ScriptBuilder.DateTimeType} NOT NULL)");
	}
}
