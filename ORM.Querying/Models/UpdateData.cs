namespace MyORM.Querying.Models;

internal class UpdateData
{
	public string TableName { get; set; }
	public List<string> Columns { get; set; }
	public List<string> Values { get; set; }
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
	public ManyToManyData? ManyToManyData { get; set; }
	public string WhereClause { get; set; }
}

internal class ManyToManyData
{
	public string TableName { get; set; }
	public string ColumnName { get; set; }
	public string ColumnValue { get; set; }
	public string ColumnName2 { get; set; }
	public string ColumnValue2 { get; set; }
}
