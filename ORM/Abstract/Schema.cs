using CLI.Methods;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.Abstract;

public class Schema
{
	public string ConnectionString { get; set; }
    private MySQL MySQL { get; set; }

    public Schema(string connectionString)
    {
        ConnectionString = connectionString;
        MySQL = new MySQL(connectionString);
    }

    public void Execute(string sql)
	{
        MySQL.ExecuteNonQuery(sql);
    }

    public DataTable Query(string sql)
    {
        return MySQL.ExecuteQuery(sql);
    }

    public bool CheckIfTableExists(string tableName)
    {
		return MySQL.CheckIfTableExists(ConnectionString, tableName);
	}

    public bool CheckIfTheLastRecord(string tableName, string columnName, string value)
    {
        return MySQL.CheckTheLastRecord(ConnectionString, tableName, columnName, value);
    }
}
