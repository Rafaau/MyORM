using MyORM.Methods;
using System.Reflection.Metadata;

namespace MyORM.DBMS;

public static class ScriptBuilder
{
	public static Database Database { get; set; }

	public static string DateTimeType
	{
		get
		{
			return Database switch
			{
				Database.MySQL => "DATETIME",
				Database.PostgreSQL => "TIMESTAMP",
				Database.MicrosoftSQLServer => "DATETIME",
				Database.SQLite => "DATETIME",
				_ => throw new Exception("Database not supported.")
			};
		}
	}

	public static string DateTimeNow
	{
		get
		{
			return Database switch
			{
				Database.MySQL => "NOW()",
				Database.PostgreSQL => "NOW()",
				Database.MicrosoftSQLServer => "GETDATE()",
				Database.SQLite => "DATETIME('now')",
				_ => throw new Exception("Database not supported.")
			};
		}
	}

	public static string BuildPrimaryKey(string pkName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"{pkName} INT NOT NULL AUTO_INCREMENT, PRIMARY KEY (Id)";
			case Database.PostgreSQL:
				return $"{pkName} SERIAL PRIMARY KEY";
			case Database.MicrosoftSQLServer:
				return $"{pkName} INT IDENTITY(1,1) PRIMARY KEY";
			case Database.SQLite:
				return $"{pkName} INTEGER PRIMARY KEY AUTOINCREMENT";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static void BuildPrimaryKey(ref string content, AttributeHelpers.Property property)
	{
		switch (Database)
		{
			case Database.MySQL:
				content += $"{property.Name} INT AUTO_INCREMENT NOT NULL, PRIMARY KEY ({property.Name})";
				break;
			case Database.PostgreSQL:
				content += $"{property.Name} SERIAL PRIMARY KEY";
				break;
			case Database.MicrosoftSQLServer:
				content += $"{property.Name} INT IDENTITY(1,1) PRIMARY KEY";
				break;
			case Database.SQLite:
				content += $"{property.Name} INTEGER PRIMARY KEY AUTOINCREMENT";
				break;
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static (string Content, string TableName) BuildManyToMany(string content, AttributeHelpers.Property property)
	{
		string script = "";
		string onDelete = "CASCADE";
		string currentModelName = property.ParentClass.ClassName;
		string relationModelName = property.Type.GetGenericArguments()[0].Name;
		string relationPropName = property.Type
			.GetGenericArguments()[0]
			.GetProperties()
			.Where(x => x.HasAttribute("ManyToMany"))
			.First().Name;
		List<string> names = new() { currentModelName, relationModelName };
		names.Sort();
		string tableName = $"{names[0].ToLower()}{property.Name}{names[1]}";
		string secondTableName = $"{names[0].ToLower()}{relationPropName}{names[1]}";
		names = new() { tableName, secondTableName };
		names.Sort();
		if (currentModelName == relationModelName)
		{
			relationModelName += "1";
			onDelete = "NO ACTION";
		}
		
		switch (Database)
		{
			case Database.MySQL:
				script += $"({currentModelName}Id INT NOT NULL, {relationModelName}Id INT NOT NULL, " +
				$"CONSTRAINT PK_{names[0]} PRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"CONSTRAINT FK_{names[0]}_{currentModelName}s_{currentModelName}Id FOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE CASCADE, " +
				$"CONSTRAINT FK_{names[0]}_{relationModelName}s_{relationModelName}Id FOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE CASCADE)";
				break;
			case Database.PostgreSQL:
				script += $"({currentModelName}Id INT NOT NULL, {relationModelName}Id INT NOT NULL, " +
				$"PRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"FOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE CASCADE, " +
				$"FOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE CASCADE)";
				break;
			case Database.MicrosoftSQLServer:
				script += $"({currentModelName}Id INT NOT NULL, {relationModelName}Id INT NOT NULL, " +
				$"PRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"CONSTRAINT FK_{names[0]}_{currentModelName}s_{currentModelName}Id FOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE {onDelete}, " +
				$"CONSTRAINT FK_{names[0]}_{relationModelName.Replace("1", "")}s_{relationModelName}Id FOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE {onDelete})";
				break;
			case Database.SQLite:
				script += $"({currentModelName}Id INT NOT NULL, {relationModelName}Id INT NOT NULL, " +
				$"PRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"FOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE CASCADE, " +
				$"FOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE CASCADE)";
				break;
			default:
				throw new Exception("Database not supported.");
		}

		return (script, names[0]);
	}

	public static void GetDataType(ref string content, AttributeHelpers.Property property)
	{
		switch (property.Type.ToString())
		{
			case "System.Int32":
				switch (Database)
				{
					case Database.MySQL:
						content += $"{property.Name} INT";
						break;
					case Database.PostgreSQL:
						content += $"{property.Name} INT";
						break;
					case Database.MicrosoftSQLServer:
						content += $"{property.Name} INT";
						break;
					case Database.SQLite:
						content += $"{property.Name} INTEGER";
						break;
					default:
						throw new Exception("Database not supported.");
				}
				break;
			case "System.String":
				switch (Database)
				{
					case Database.MySQL:
						content += $"{property.Name} VARCHAR(255)";
						break;
					case Database.PostgreSQL:
						content += $"{property.Name} VARCHAR(255)";
						break;
					case Database.MicrosoftSQLServer:
						content += $"{property.Name} NVARCHAR(255)";
						break;
					case Database.SQLite:
						content += $"{property.Name} TEXT";
						break;
					default:
						throw new Exception("Database not supported.");
				}
				break;
			case "System.DateTime":
				switch (Database)
				{
					case Database.MySQL:
						content += $"{property.Name} DATETIME";
						break;
					case Database.PostgreSQL:
						content += $"{property.Name} TIMESTAMP";
						break;
					case Database.MicrosoftSQLServer:
						content += $"{property.Name} DATETIME";
						break;
					case Database.SQLite:
						content += $"{property.Name} DATETIME";
						break;
					default:
						throw new Exception("Database not supported.");
				}
				break;
			case "System.Boolean":
				content += $"{property.Name} BOOLEAN";
				break;
			default:
				break;
		}
	}

	public static string BuildSelect(string tableName, string columnName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"{tableName}.{columnName} AS '{tableName}.{columnName}'";
			case Database.PostgreSQL:
				return $"{tableName}.{columnName} AS \"{tableName}.{columnName}\"";
			case Database.MicrosoftSQLServer:
				return $"{tableName}.{columnName} AS [{tableName}.{columnName}]";
			case Database.SQLite:
				return $"{tableName}.{columnName} AS '{tableName}.{columnName}'";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static string BuildIdentity(string tableName, string columnName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return "SELECT LAST_INSERT_ID();";
			case Database.PostgreSQL:
				return $"SELECT currval(pg_get_serial_sequence('{tableName}','{columnName.ToLower()}'));";
			case Database.MicrosoftSQLServer:
				return $"SELECT IDENT_CURRENT('{tableName}');";
			case Database.SQLite:
				return "SELECT last_insert_rowid();";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static string BuildForeignKey(string tableName, AttributeHelpers.Property property)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"ADD FOREIGN KEY ({property.Name}Id) ";
			case Database.PostgreSQL:
				return $"ADD FOREIGN KEY ({property.Name}Id) ";
			case Database.MicrosoftSQLServer:
				return 
					$"CONSTRAINT FK_{tableName}_{property.Type.FullName.Split('.').Last().ToLower() + "s"}" +
					$"_{property.Name}Id FOREIGN KEY ({property.Name}Id) ";
			case Database.SQLite:
				return $"FOREIGN KEY ({property.Name}Id) ";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static string SelectDatabase(string dbName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"SHOW DATABASES LIKE '{dbName}'";
			case Database.PostgreSQL:
				return $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'";
			case Database.MicrosoftSQLServer:
				return $"SELECT database_id FROM sys.databases WHERE name = '{dbName}'";
			case Database.SQLite:
				return $"SELECT * FROM pragma_database_list WHERE name = '{dbName}'";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static string SelectTable(string tableName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"SHOW TABLES LIKE '{tableName}'";
			case Database.PostgreSQL:
				return $"SELECT * FROM information_schema.tables WHERE table_name = '{tableName}'";
			case Database.MicrosoftSQLServer:
				return $"SELECT * FROM sys.tables WHERE name = '{tableName}'";
			case Database.SQLite:
				return $"SELECT * FROM sqlite_master WHERE type = 'table' AND name = '{tableName}'";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static string SelectLastRecord(string columnName, string tableName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"SELECT {columnName} FROM {tableName} ORDER BY Id DESC LIMIT 1";
			case Database.PostgreSQL:
				return $"SELECT {columnName} FROM {tableName} ORDER BY Id DESC LIMIT 1";
			case Database.MicrosoftSQLServer:
				return $"SELECT TOP 1 {columnName} FROM {tableName} ORDER BY Id DESC";
			case Database.SQLite:
				return $"SELECT {columnName} FROM {tableName} ORDER BY Id DESC LIMIT 1";
			default:
				throw new Exception("Database not supported.");
		}
	}
}


