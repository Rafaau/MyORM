using System.Data;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data.Common;
using MyORM.DBMS;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace MyORM.Methods;

/// <summary>
/// Class that manages the database connection and operations.
/// </summary>
internal class DatabaseManager : IDisposable
{
    /// <summary>
    /// Gets or sets the database connection.
    /// </summary>
    private DbConnection Connection { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    private string DbName { get; set; }

    /// <summary>
    /// Gets or sets the database transaction.
    /// </summary>
    private DbTransaction? Transaction { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to keep the connection open.
    /// </summary>
    private bool KeepConnectionOpen { get; set; }

    /// <summary>
    /// Constructor that initializes the database manager.
    /// </summary>
    /// <param name="accessLayer">Access layer instance</param>
    /// <exception cref="Exception">Exception that is thrown when the database is not supported.</exception>
    public DatabaseManager(AccessLayer accessLayer)
	{
		DbName = GetDbName(accessLayer.ConnectionString);
		KeepConnectionOpen = accessLayer.Options.KeepConnectionOpen;

		switch (accessLayer.Options.Database)
		{
			case Database.MySQL:
				Connection = new MySqlConnection(accessLayer.ConnectionString);
				break;
			case Database.PostgreSQL:
				Connection = new NpgsqlConnection(accessLayer.ConnectionString);
				break;
			case Database.MicrosoftSQLServer:
				Connection = new SqlConnection(accessLayer.ConnectionString);
				break;
			case Database.SQLite:
				Connection = new SqliteConnection(accessLayer.ConnectionString);
				break;
			default:
				throw new Exception("Database not supported.");
		}

		Connection.Open();
		CheckDatabase();
		Connection.Close();
	}

    /// <summary>
    /// Opens the connection to the database.
    /// </summary>
    public void OpenConnection() => Connection.Open();

    /// <summary>
    /// Closes the connection to the database.
    /// </summary>
    public void CloseConnection() => Connection.Close();

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    public void BeginTransaction()
	{
		if (Connection.State == ConnectionState.Closed)
			Connection.Open();

		if (Transaction is null)
			Transaction = Connection.BeginTransaction();
	}

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    public void CommitTransaction()
	{
		Transaction?.Commit();
		Transaction = null;
		if (!KeepConnectionOpen)
			Connection.Close();
	}

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    public void RollbackTransaction()
	{
		Transaction?.Rollback();
		Transaction = null;
		if (!KeepConnectionOpen)
			Connection.Close();
	}

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="sqlCommandText">Command text</param>
    /// <returns>Returns the number of rows affected</returns>
    public int ExecuteNonQuery(string sqlCommandText)
	{
		try
		{
			if (Connection.State == ConnectionState.Closed)
			{
				Connection.Open();
			}
			using (DbCommand command = Connection.CreateCommand())
			{
				command.CommandText = sqlCommandText;
				command.Transaction = Transaction;
				return command.ExecuteNonQuery();
			}
		}
		catch (Exception e)
		{
			RollbackTransaction();
			throw e;
		}
		finally
		{
			if (!KeepConnectionOpen)
				Connection.Close();
		}
	}

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="sqlCommandText">Command text</param>
    /// <returns>Returns the result of the query</returns>
    public DataTable ExecuteQuery(string sqlCommandText)
	{
		try
		{
			if (Connection.State == ConnectionState.Closed)
			{
				Connection.Open();
			}
			DataTable dataTable = new DataTable();

			using (DbCommand command = Connection.CreateCommand())
			{
				command.CommandText = sqlCommandText;
				command.Transaction = Transaction;

				using (DbDataReader reader = command.ExecuteReader())
				{
					dataTable.Load(reader);
				}
			}
			return dataTable;
		}
		catch (Exception e)
		{
			RollbackTransaction();
			throw e;
		}
		finally
		{
			if (!KeepConnectionOpen)
				Connection.Close();
		}
	}

    /// <summary>
    /// Checks if a table exists.
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <returns>Returns a value indicating whether the table exists</returns>
    public bool CheckIfTableExists(string tableName)
	{
		try
		{
			if (Connection.State == ConnectionState.Closed)
			{
				Connection.Open();
			}

			using (DbCommand command = Connection.CreateCommand())
			{
				command.CommandText = ScriptBuilder.SelectTable(tableName);
				command.Transaction = Transaction;

				using (var reader = command.ExecuteReader())
				{
					return reader.HasRows;
				}
			}
		}
		finally
		{
			if (!KeepConnectionOpen)
				Connection.Close();
		}
	}

    /// <summary>
    /// Checks if the last record of a table has a specific value.
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column</param>
    /// <param name="value">Value to check</param>
    /// <returns>Returns a value indicating whether the last record has the specific value</returns>
    public bool CheckTheLastRecord(string tableName, string columnName, string value)
	{
		if (Connection.State == ConnectionState.Closed)
		{
			Connection.Open();
		}
		using (DbCommand command = Connection.CreateCommand())
		{
			command.CommandText = ScriptBuilder.SelectLastRecord(columnName, tableName);
			command.Transaction = Transaction;

			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					string columnValue = reader.GetString(0);
					if (!KeepConnectionOpen)
						Connection.Close();
					return columnValue.Equals(value);
				}
				else
				{
					if (!KeepConnectionOpen)
						Connection.Close();
					return false;
				}
			}
		}
	}

    /// <summary>
    /// Checks if the database exists and creates it if it does not.
    /// </summary>
    private void CheckDatabase()
	{
		if (!DatabaseExists(Connection, DbName))
		{
			CreateDatabase(Connection, DbName);
		}

		Connection.ChangeDatabase(DbName);
	}

    /// <summary>
    /// Checks if the database exists.
    /// </summary>
    /// <param name="connection">Connection to the database</param>
    /// <param name="dbName">Name of the database</param>
    /// <returns>Returns a value indicating whether the database exists</returns>
    private bool DatabaseExists(DbConnection connection, string dbName)
	{
		using (DbCommand command = connection.CreateCommand())
		{
			command.CommandText = ScriptBuilder.SelectDatabase(dbName);
			command.Transaction = Transaction;

			using (var reader = command.ExecuteReader())
			{
				return reader.HasRows;
			}
		}
	}

    /// <summary>
    /// Creates a database.
    /// </summary>
    /// <param name="connection">Connection to the database</param>
    /// <param name="dbName">Name of the database</param>
    private void CreateDatabase(DbConnection connection, string dbName)
	{
		using (DbCommand command = connection.CreateCommand())
		{
			command.CommandText = $"CREATE DATABASE {dbName};";
			command.Transaction = Transaction;
			command.ExecuteNonQuery();
		}
	}

    /// <summary>
    /// Gets the database name from the connection string.
    /// </summary>
    /// <param name="connectionString">Connection string</param>
    /// <returns>Returns the database name</returns>
    private string GetDbName(string connectionString)
	{
		return connectionString.Split(';')
			.Select(x => x.Split('='))
			.Where(x => x[0].Equals("Database"))
			.Select(x => x[1])
			.FirstOrDefault();
	}

    /// <summary>
    /// Disposes the database manager.
    /// </summary>
    public void Dispose()
	{
		Connection.Dispose();
	}
}