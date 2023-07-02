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
}