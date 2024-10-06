namespace MyORM.Models;

public class ColumnStatement
{
	public string PropertyName { get; set; }
	public string ColumnName { get; set; }
	public string Type { get; set; }

	public ColumnStatement(string propertyName, string columnName, string type)
	{
		PropertyName = propertyName;
		ColumnName = columnName;
		Type = type;
	}
}

