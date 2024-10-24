namespace MyORM.Models;

public class ModelStatement
{
	public string Name { get; set; }
	public string TableName { get; set; }
	public List<ColumnStatement> Columns { get; set; } = new();
	public List<RelationStatement> Relationships { get; set; } = new();

	public ModelStatement(string name, string tableName, List<ColumnStatement> columns, List<RelationStatement> relations = null)
	{
		Name = name;
		TableName = tableName;
		Columns = columns;
		Relationships = relations;
	}

	public ColumnStatement GetColumn(string propertyName) 
		=> Columns.Where(x => x.PropertyName == propertyName).SingleOrDefault();

	public string GetColumnName(string propertyName) 
		=> Columns.Where(x => x.PropertyName == propertyName).Select(x => x.ColumnName).SingleOrDefault();

	public string GetPrimaryKeyPropertyName() 
		=> Columns.Where(x => x.PropertyOptions.Contains("PRIMARY KEY")).Select(x => x.PropertyName).SingleOrDefault();
}

public static class Extensions
{
	public static ModelStatement GetModelStatement(this List<ModelStatement> statements, string name) 
		=> statements.Where(x => x.Name == name).SingleOrDefault();
}

