using MySql.Data.MySqlClient;
using System.Data.Common;

namespace CLI.Methods;
internal static class MySQL
{
	public static void ApplyMigration(string connectionString, string migrationScript)
	{
		using (DbConnection connection = new MySqlConnection(connectionString))
		{
			connection.Open();
			using (DbCommand command = connection.CreateCommand())
			{
				command.CommandText = migrationScript;
				command.ExecuteNonQuery();
			}
		}
	}

	public static bool CheckIfTableExists(string connectionString, string tableName)
	{
		using (var connection = new MySqlConnection(connectionString))
		{
			connection.Open();
			using (DbCommand command = new MySqlCommand($"SHOW TABLES LIKE '{tableName}';", connection))
			{
				using (var reader = command.ExecuteReader())
				{
                    return reader.HasRows;
				}
			}
		}
	}

	public static bool CheckTheLastRecord(string connectionString, string tableName, string columnName, string value)
	{
		using (var connection = new MySqlConnection(connectionString))
		{
			connection.Open();
			using (DbCommand command = new MySqlCommand($"SELECT {columnName} FROM {tableName} ORDER BY Id DESC LIMIT 1;", connection))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						string columnValue = reader.GetString(0);
						return columnValue.Equals(value);
					} 
					else
					{
						return false;
					}
				}
			}
		}
	}
}