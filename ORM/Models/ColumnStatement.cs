namespace MyORM.Models;

public class ColumnStatement
{
	public string PropertyName { get; set; }
	public string ColumnName { get; set; }
	public string PropertyOptions { get; set; }

	public ColumnStatement(string propertyName, string columnName, string propertyOptions)
	{
		PropertyName = propertyName;
		ColumnName = columnName;
		PropertyOptions = propertyOptions;
	}
}

