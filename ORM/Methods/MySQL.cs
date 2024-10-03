using System.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace MyORM.Methods;
internal class MySQL : IDisposable
{
	private DbConnection Connection { get; set; }
	private MySqlConnectionStringBuilder Builder { get; set; }
	private string DbName { get; set; }
	private DbTransaction? Transaction { get; set; }
	private bool KeepConnectionOpen { get; set; }

	public MySQL(string connectionString, bool keepConnectionOpen)
	{
		Builder = new MySqlConnectionStringBuilder(connectionString);
		DbName = Builder.Database;
		Builder.Database = "";
		KeepConnectionOpen = keepConnectionOpen;

		Connection = new MySqlConnection(Builder.ConnectionString);
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
			using (DbCommand command = new MySqlCommand($"SHOW TABLES LIKE '{tableName}';", (MySqlConnection)Connection))
			{
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
		using (DbCommand command = new MySqlCommand($"SELECT {columnName} FROM {tableName} ORDER BY Id DESC LIMIT 1;", (MySqlConnection)Connection))
		{
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

	private static bool DatabaseExists(DbConnection connection, string dbName)
	{
		using (DbCommand command = connection.CreateCommand())
		{
			command.CommandText = $"SHOW DATABASES LIKE '{dbName}'";
			using (var reader = command.ExecuteReader())
			{
				return reader.HasRows;
			}
		}
	}

	private static void CreateDatabase(DbConnection connection, string dbName)
	{
		using (DbCommand command = connection.CreateCommand())
		{
			command.CommandText = $"CREATE DATABASE {dbName};";
			command.ExecuteNonQuery();
		}
	}

	public void Dispose()
	{
		Connection.Dispose();
	}
}