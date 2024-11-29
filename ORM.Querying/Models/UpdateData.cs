namespace MyORM.Querying.Models;

/// <summary>
/// Class that represents the update data.
/// </summary>
internal class UpdateData
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the list of columns.
    /// </summary>
    public List<string> Columns { get; set; }

    /// <summary>
    /// Gets or sets the list of values.
    /// </summary>
    public List<string> Values { get; set; }

    /// <summary>
    /// Gets the list of column values.
    /// </summary>
    public List<string> ColumnValues
	{
		get
		{
			List<string> columnValues = new();
			for (int i = 0; i < Columns.Count; i++)
			{
				columnValues.Add($"{Columns[i]} = {Values[i]}");
			}
			return columnValues;
		}
	}

    /// <summary>
    /// Gets or sets the many to many data.
    /// </summary>
    public ManyToManyData? ManyToManyData { get; set; }

    /// <summary>
    /// Gets or sets the where clause.
    /// </summary>
	public string WhereClause { get; set; }

    /// <summary>
    /// Gets or sets the primary key column name.
    /// </summary>
	public string PrimaryKeyColumnName { get; set; }

    /// <summary>
    /// Gets or sets the relation update.
    /// </summary>
	public UpdateData RelationUpdate { get; set; }

    /// <summary>
    /// Gets or sets the foreign key column name.
    /// </summary>
	public string ForeignKeyColumnName { get; set; }
}

/// <summary>
/// Class that represents the many to many data.
/// </summary>
internal class ManyToManyData
{
    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
	public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the column name.
    /// </summary>
	public string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the column value.
    /// </summary>
	public string ColumnValue { get; set; }

    /// <summary>
    /// Gets or sets the column name 2.
    /// </summary>
	public string ColumnName2 { get; set; }

    /// <summary>
    /// Gets or sets the column value 2.
    /// </summary>
	public string ColumnValue2 { get; set; }
}
