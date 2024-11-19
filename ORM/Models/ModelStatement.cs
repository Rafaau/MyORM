using MyORM.DBMS;
using static MyORM.Methods.AttributeHelpers;

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

	public string GetPrimaryKeyColumnName() 
        => Columns.Where(x => x.PropertyOptions.Contains("PRIMARY KEY")).Select(x => x.ColumnName).SingleOrDefault();

	public RelationStatement GetRelationStatement(string propertyName) 
		=> Relationships.Where(x => x.PropertyName == propertyName).SingleOrDefault();
}

public static class Extensions
{
	public static ModelStatement GetModelStatement(this List<ModelStatement> statements, string name) 
		=> statements.Where(x => x.Name == name).SingleOrDefault();

	public static bool ContainsProperty(this List<ColumnStatement> columns, Property property) 
		=> columns.Any(y => y.PropertyName == property.Name);

	public static bool ColumnNameHasChanged(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name && x.ColumnName != property.ColumnName);

	public static bool PropertyOptionsHaveChanged(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name 
		&& x.PropertyOptions.RemoveUnique() != ScriptBuilder
			.HandlePropertyOptions(property, Operation.Create)
			.RemoveFormatting()
			.RemoveUnique()
			.Substring(property.ColumnName.Length + 1));

	public static bool ColumnBecameUnique(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name 
			&& !x.PropertyOptions.Contains("UNIQUE") 
			&& ScriptBuilder.HandlePropertyOptions(property, Operation.Create).Contains("UNIQUE"));

	public static bool ColumnLostUnique(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name 
		            && x.PropertyOptions.Contains("UNIQUE") 
		            && !ScriptBuilder.HandlePropertyOptions(property, Operation.Create).Contains("UNIQUE"));
}

