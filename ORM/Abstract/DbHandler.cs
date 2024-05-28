using CLI.Methods;
using System.Data;

namespace ORM.Abstract;

public class DbHandler
{
	public string ConnectionString { get; set; }
    private MySQL MySQL { get; set; }
    private bool KeepConnectionOpen { get; set; }

    public DbHandler(string connectionString, bool keepConnectionOpen = false)
    {
        ConnectionString = connectionString;
        MySQL = new MySQL(connectionString, keepConnectionOpen);
        KeepConnectionOpen = keepConnectionOpen;
    }

    public void CloseConnection() => MySQL.CloseConnection();

    public void BeginTransaction()
    {
		MySQL.BeginTransaction();
	}

    public void CommitTransaction()
    {
        MySQL.CommitTransaction();
    }

    public void Execute(string sql)
	{   
        Console.WriteLine(sql);
        MySQL.ExecuteNonQuery(sql);
    }

    public void ExecuteWithTransaction(string sql)
    {
		BeginTransaction();
		Execute(sql);
		CommitTransaction();
	}

    public DataTable Query(string sql)
    {
        return MySQL.ExecuteQuery(sql);
    }

    public bool CheckIfTableExists(string tableName)
    {
		return MySQL.CheckIfTableExists(tableName);
	}

    public bool CheckIfTheLastRecord(string tableName, string columnName, string value)
    {
        return MySQL.CheckTheLastRecord(tableName, columnName, value);
    }
}
