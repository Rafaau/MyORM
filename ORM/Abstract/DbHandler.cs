using MyORM.Methods;
using System.Data;

namespace MyORM.Abstract;

/// <summary>
/// Class that handles the database connection and operations.
/// </summary>
public class DbHandler
{
	public string ConnectionString { get; set; }
	public AccessLayer AccessLayer { get; set; }
	private MySQL MySQL { get; set; }
	private bool KeepConnectionOpen { get; set; } = false;

	/// <summary>
	/// Constructor that initializes the connection string and the MySQL object.
	/// </summary>
	/// <param name="accessLayer"><c>AccessLayer</c> instance.</param>
	public DbHandler(AccessLayer accessLayer)
	{
		AccessLayer = accessLayer;
		ConnectionString = accessLayer.ConnectionString;
		KeepConnectionOpen = accessLayer.Options.KeepConnectionOpen;
		MySQL = new MySQL(ConnectionString, KeepConnectionOpen);
	}

	/// <summary>
	/// Constructor that initializes the connection string and the MySQL object.
	/// </summary>
	/// <param name="connectionString">Connection string to database.</param>
	public DbHandler(string connectionString)
	{
		ConnectionString = connectionString;
		MySQL = new MySQL(ConnectionString, KeepConnectionOpen);
	}

	public void OpenConnection() => MySQL.OpenConnection();

	public void CloseConnection() => MySQL.CloseConnection();

	public void BeginTransaction()
	{
		MySQL.BeginTransaction();
	}

	public void CommitTransaction()
	{
		MySQL.CommitTransaction();
	}

	public int Execute(string sqlCommandText)
	{
		Console.WriteLine(sqlCommandText);
		return MySQL.ExecuteNonQuery(sqlCommandText);
	}

	public void ExecuteWithTransaction(string sqlCommandText)
	{
		BeginTransaction();
		Execute(sqlCommandText);
		CommitTransaction();
	}

	public DataTable Query(string sqlCommandText)
	{
		return MySQL.ExecuteQuery(sqlCommandText);
	}

	public bool CheckIfTableExists(string tableName)
	{
		return MySQL.CheckIfTableExists(tableName);
	}

	public bool CheckTheLastRecord(string tableName, string columnName, string value)
	{
		return MySQL.CheckTheLastRecord(tableName, columnName, value);
	}
}
