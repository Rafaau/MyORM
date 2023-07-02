using MySql.Data.MySqlClient;
using System;
using System.Data.Common;

namespace CLI.Methods;

internal class MySQL
{
	public void ApplyMigration(string connectionString, string migrationScript)
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
