namespace MyORM.Models;

/// <summary>
/// Class that represents a relation statement.
/// </summary>
public class RelationStatement
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
	public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
	public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the first column name.
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the second column name.
    /// </summary>
    public string? ColumnName_1 { get; set; }

    /// <summary>
    /// Constructor for the RelationStatement class.
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the first column</param>
    /// <param name="columnName1">Name of the second column</param>
    public RelationStatement(string propertyName, string tableName, string columnName, string? columnName1 = null)
    {
        PropertyName = propertyName;
        TableName = tableName;
        ColumnName = columnName;
        ColumnName_1 = columnName1;
    }
}
