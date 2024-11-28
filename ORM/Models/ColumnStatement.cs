namespace MyORM.Models;

/// <summary>
/// Class that represents a column statement for model statement.
/// </summary>
public class ColumnStatement
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the property options.
    /// </summary>
	public string PropertyOptions { get; set; }

    /// <summary>
    /// Constructor for the ColumnStatement class.
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <param name="columnName">Name of the column</param>
    /// <param name="propertyOptions">Options of the property</param>
	public ColumnStatement(string propertyName, string columnName, string propertyOptions)
	{
		PropertyName = propertyName;
		ColumnName = columnName;
		PropertyOptions = propertyOptions;
	}
}

