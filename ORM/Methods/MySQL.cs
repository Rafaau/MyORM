using System.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace CLI.Methods;
internal class MySQL
{
	private DbConnection Connection { get; set; }
    private MySqlConnectionStringBuilder Builder { get; set; }
	private string DbName { get; set; }

    public MySQL(string connectionString)
    {
        Builder = new MySqlConnectionStringBuilder(connectionString);
        DbName = Builder.Database;
		Builder.Database = "";

		Connection = new MySqlConnection(Builder.ConnectionString);
        Connection.Open();
        CheckDatabase();
    }

	public void ExecuteNonQuery(string sqlCommand)
	{
        using (DbCommand command = Connection.CreateCommand())
		{
			command.CommandText = sqlCommand;
			command.ExecuteNonQuery();
		}
    }

    public DataTable ExecuteQuery(string sqlCommand)
    {
        DataTable dataTable = new DataTable();

        using (DbCommand command = Connection.CreateCommand())
        {
            command.CommandText = sqlCommand;

            using (DbDataReader reader = command.ExecuteReader())
            {
                dataTable.Load(reader);
            }
        }

        return dataTable;
    }

    public bool CheckIfTableExists(string connectionString, string tableName)
	{
        using (DbCommand command = new MySqlCommand($"SHOW TABLES LIKE '{tableName}';", (MySqlConnection)Connection))
		{
			using (var reader = command.ExecuteReader())
			{
                return reader.HasRows;
			}
        }
	}

	public bool CheckTheLastRecord(string connectionString, string tableName, string columnName, string value)
	{
        using (DbCommand command = new MySqlCommand($"SELECT {columnName} FROM {tableName} ORDER BY Id DESC LIMIT 1;", (MySqlConnection)Connection))
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
}