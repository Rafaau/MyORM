using CLI.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.Abstract;

public class Schema
{
	public string connectionString { get; set; }

    public Schema(string _connectionString)
    {
        connectionString = _connectionString;
    }

    public void Execute(string sql)
	{
        MySQL.ApplyMigration(connectionString, sql);
    }

    public bool CheckIfTableExists(string tableName)
    {
		return MySQL.CheckIfTableExists(connectionString, tableName);
	}

    public bool CheckIfTheLastRecord(string tableName, string columnName, string value)
    {
        return MySQL.CheckTheLastRecord(connectionString, tableName, columnName, value);
    }
}
