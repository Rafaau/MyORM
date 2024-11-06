using System.Data;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data.Common;
using MyORM.DBMS;
using Microsoft.Data.SqlClient;
using System.Transactions;
using Microsoft.Data.Sqlite;

namespace MyORM.Methods;
internal class DatabaseManager : IDisposable
{
	private DbConnection Connection { get; set; }
	private string DbName { get; set; }
	private DbTransaction? Transaction { get; set; }
	private bool KeepConnectionOpen { get; set; }

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

	public void OpenConnection() => Connection.Open();

	public void CloseConnection() => Connection.Close();

	public void BeginTransaction()
	{
		if (Connection.State == ConnectionState.Closed)
		{
			Connection.Open();
		}
		Transaction = Connection.BeginTransaction();
	}

	public void CommitTransaction()
	{
		Transaction?.Commit();
		Transaction = null;
		if (!KeepConnectionOpen)
			Connection.Close();
	}

	public void RollbackTransaction()
	{
		Transaction?.Rollback();
		Transaction = null;
		if (!KeepConnectionOpen)
			Connection.Close();
	}

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

	private void CheckDatabase()
	{
		if (!DatabaseExists(Connection, DbName))
		{
			CreateDatabase(Connection, DbName);
		}

		Connection.ChangeDatabase(DbName);
	}

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

	private void CreateDatabase(DbConnection connection, string dbName)
	{
		using (DbCommand command = connection.CreateCommand())
		{
			command.CommandText = $"CREATE DATABASE {dbName};";
			command.Transaction = Transaction;
			command.ExecuteNonQuery();
		}
	}

	private string GetDbName(string connectionString)
	{
		return connectionString.Split(';')
			.Select(x => x.Split('='))
			.Where(x => x[0].Equals("Database"))
			.Select(x => x[1])
			.FirstOrDefault();
	}

	public void Dispose()
	{
		Connection.Dispose();
	}
}