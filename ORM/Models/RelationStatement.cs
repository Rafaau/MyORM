namespace MyORM.Models;

public class RelationStatement
{
	public string PropertyName { get; set; }
	public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string? ColumnName_1 { get; set; }

    public RelationStatement(string propertyName, string tableName, string columnName, string? columnName1 = null)
    {
        PropertyName = propertyName;
        TableName = tableName;
        ColumnName = columnName;
        ColumnName_1 = columnName1;
    }
}
