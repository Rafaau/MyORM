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
}