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
				content += $"{property.ColumnName} INT AUTO_INCREMENT NOT NULL, PRIMARY KEY ({property.Name})";
				break;
			case Database.PostgreSQL:
				content += $"{property.ColumnName} SERIAL PRIMARY KEY";
				break;
			case Database.MicrosoftSQLServer:
				content += $"{property.ColumnName} INT IDENTITY(1,1) PRIMARY KEY";
				break;
			case Database.SQLite:
				content += $"{property.ColumnName} INTEGER PRIMARY KEY AUTOINCREMENT";
				break;
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static (string Content, string TableName) BuildManyToMany(AttributeHelpers.Property property)
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
				script += $"(" +
				$"\r\n\t\t\t\t{currentModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\t{relationModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\tCONSTRAINT PK_{names[0]} PRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"\r\n\t\t\t\tCONSTRAINT FK_{names[0]}_{currentModelName}s_{currentModelName}Id FOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE CASCADE, " +
				$"\r\n\t\t\t\tCONSTRAINT FK_{names[0]}_{relationModelName}s_{relationModelName}Id FOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE CASCADE)";
				break;
			case Database.PostgreSQL:
				script += $"(" +
				$"\r\n\t\t\t\t{currentModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\t{relationModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\tPRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"\r\n\t\t\t\tFOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE CASCADE, " +
				$"\r\n\t\t\t\tFOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE CASCADE)";
				break;
			case Database.MicrosoftSQLServer:
				script += $"(" +
				$"\r\n\t\t\t\t{currentModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\t{relationModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\tPRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"\r\n\t\t\t\tCONSTRAINT FK_{names[0]}_{currentModelName}s_{currentModelName}Id FOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE {onDelete}, " +
				$"\r\n\t\t\t\tCONSTRAINT FK_{names[0]}_{relationModelName.Replace("1", "")}s_{relationModelName}Id FOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE {onDelete})";
				break;
			case Database.SQLite:
				script += $"(" +
				$"\r\n\t\t\t\t{currentModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\t{relationModelName}Id INT NOT NULL, " +
				$"\r\n\t\t\t\tPRIMARY KEY ({currentModelName}Id, {relationModelName}Id), " +
				$"\r\n\t\t\t\tFOREIGN KEY ({currentModelName}Id) REFERENCES {currentModelName.ToLower()}s(Id) ON DELETE CASCADE, " +
				$"\r\n\t\t\t\tFOREIGN KEY ({relationModelName}Id) REFERENCES {relationModelName.Replace("1", "").ToLower()}s(Id) ON DELETE CASCADE)";
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
						content += $"{property.ColumnName} INT";
						break;
					case Database.PostgreSQL:
						content += $"{property.ColumnName} INT";
						break;
					case Database.MicrosoftSQLServer:
						content += $"{property.ColumnName} INT";
						break;
					case Database.SQLite:
						content += $"{property.ColumnName} INTEGER";
						break;
					default:
						throw new Exception("Database not supported.");
				}
				break;
			case "System.String":
				switch (Database)
				{
					case Database.MySQL:
						content += $"{property.ColumnName} VARCHAR(255)";
						break;
					case Database.PostgreSQL:
						content += $"{property.ColumnName} VARCHAR(255)";
						break;
					case Database.MicrosoftSQLServer:
						content += $"{property.ColumnName} NVARCHAR(255)";
						break;
					case Database.SQLite:
						content += $"{property.ColumnName} TEXT";
						break;
					default:
						throw new Exception("Database not supported.");
				}
				break;
			case "System.DateTime":
				switch (Database)
				{
					case Database.MySQL:
						content += $"{property.ColumnName} DATETIME";
						break;
					case Database.PostgreSQL:
						content += $"{property.ColumnName} TIMESTAMP";
						break;
					case Database.MicrosoftSQLServer:
						content += $"{property.ColumnName} DATETIME";
						break;
					case Database.SQLite:
						content += $"{property.ColumnName} DATETIME";
						break;
					default:
						throw new Exception("Database not supported.");
				}
				break;
			case "System.Boolean":
				content += $"{property.ColumnName} BOOLEAN";
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
				return $"\r\n\t\t\t\tADD FOREIGN KEY ({property.Name}Id) ";
			case Database.PostgreSQL:
				return $"\r\n\t\t\t\tADD FOREIGN KEY ({property.Name}Id) ";
			case Database.MicrosoftSQLServer:
				return 
					$"\r\n\t\t\t\tCONSTRAINT FK_{tableName}_{property.Type.FullName.Split('.').Last().ToLower() + "s"}" +
					$"_{property.Name}Id FOREIGN KEY ({property.Name}Id) ";
			case Database.SQLite:
				return $"\r\n\t\t\t\tFOREIGN KEY ({property.Name}Id) ";
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

	public static string Rename(string tableName, string oldColumnName, string newColumnName)
	{
		switch (Database)
		{
			case Database.MySQL:
				return $"ALTER TABLE {tableName} CHANGE {oldColumnName} {newColumnName}";
			case Database.PostgreSQL:
				return $"ALTER TABLE {tableName} RENAME COLUMN {oldColumnName} TO {newColumnName}";
			case Database.MicrosoftSQLServer:
				return $"EXEC sp_rename '{tableName}.{oldColumnName}', '{newColumnName}', 'COLUMN'";
			case Database.SQLite:
				return $"ALTER TABLE {tableName} RENAME COLUMN {oldColumnName} TO {newColumnName}";
			default:
				throw new Exception("Database not supported.");
		}
	}

	public static string HandlePropertyOptions(AttributeHelpers.Property prop, Operation operation)
	{
		string content = "";

		if (prop.Attributes.Select(x => x.Name).Contains("PrimaryGeneratedColumn"))
			BuildPrimaryKey(ref content, prop);
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("OneToOne"))
		{
			string relationshipString = prop.HasRelationship(Relationship.Optional)
				? "NULL"
				: "NOT NULL, " +
				BuildForeignKey(prop.ParentClass.TableName, prop) +
				$"REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)" +
				(prop.HasCascadeOption() ? " ON DELETE CASCADE" : "");

			content += $"{prop.ColumnName} INT UNIQUE {relationshipString}";
		}
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("ManyToOne"))
			content += 
				$"{prop.ColumnName} INT, {(operation == Operation.Alter ? "ADD" : "")} " +
				$"{BuildForeignKey(prop.ParentClass.TableName, prop).Replace("ADD", "")} " +
				$"REFERENCES {prop.Type.FullName.Split('.').Last().ToLower() + "s"}(Id)";
		else if (prop.Attributes.Select(x => x.Name).Single().Contains("ManyToMany"))
		{
			content = BuildManyToMany(prop).Content;
		}
		else if (prop.Attributes.Select(x => x.Name).Contains("Column"))
		{
			GetDataType(ref content, prop);

			if (prop.AttributeProps.Any(x => x.Key == "Unique"
				&& (bool)x.Value == true)
				&& operation == Operation.Create)
				content += " UNIQUE";

			if (prop.AttributeProps.Any(x => x.Key == "Nullable" && (bool)x.Value == false))
				content += " NOT NULL";

			if (prop.AttributeProps.Any(x => x.Key == "DefaultValue" && x.Value != null))
			{
				var defaultValue = prop.AttributeProps.First(x => x.Key == "DefaultValue").Value;
				string defaultValueString = defaultValue.GetType() == typeof(string) ? $"'{defaultValue}'" : defaultValue.ToString();
				content += $" DEFAULT {defaultValueString}";
			}
		}

		return content;
	}
}

public static class Extensions
{
    public static string RemoveFormatting(this string content)
	{
		content = content.Replace("\r\n\t\t\t\t", "");
		return content.Replace("  ", " ");
	} 

	public static string RemoveUnique(this string content)
	{
		return content.Replace(" UNIQUE", "");
	}
}


