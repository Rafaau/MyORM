using MyORM.DBMS;
using static MyORM.Methods.AttributeHelpers;

namespace MyORM.Models;

/// <summary>
/// Class that represents a model statement.
/// </summary>
public class ModelStatement
{
    /// <summary>
    /// Gets or sets the name of the model.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the list of columns.
    /// </summary>
    public List<ColumnStatement> Columns { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of relationships.
    /// </summary>
    public List<RelationStatement> Relationships { get; set; } = new();

    /// <summary>
    /// Constructor for the ModelStatement class.
    /// </summary>
    /// <param name="name">Name of the model</param>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columns">List of columns</param>
    /// <param name="relations">List of relationships</param>
    public ModelStatement(string name, string tableName, List<ColumnStatement> columns, List<RelationStatement> relations = null)
	{
		Name = name;
		TableName = tableName;
		Columns = columns;
		Relationships = relations;
	}

    /// <summary>
    /// Gets the column statement by property name.
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Returns the column statement</returns>
    public ColumnStatement GetColumn(string propertyName) 
		=> Columns.Where(x => x.PropertyName == propertyName).SingleOrDefault();

    /// <summary>
    /// Gets the column name by property name.
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Returns the column name</returns>
    public string GetColumnName(string propertyName) 
		=> Columns.Where(x => x.PropertyName == propertyName).Select(x => x.ColumnName).SingleOrDefault();

    /// <summary>
    /// Gets the primary key property name of the model.
    /// </summary>
    /// <returns>Returns the primary key property name</returns>
	public string GetPrimaryKeyPropertyName() 
		=> Columns.Where(x => x.PropertyOptions.Contains("PRIMARY KEY")).Select(x => x.PropertyName).SingleOrDefault();

    /// <summary>
    /// Gets the primary key column name of the model.
    /// </summary>
    /// <returns>Returns the primary key column name</returns>
	public string GetPrimaryKeyColumnName() 
        => Columns.Where(x => x.PropertyOptions.Contains("PRIMARY KEY")).Select(x => x.ColumnName).SingleOrDefault();

    /// <summary>
    /// Gets the relation statement by property name.
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Returns the relation statement</returns>
	public RelationStatement GetRelationStatement(string propertyName) 
		=> Relationships.Where(x => x.PropertyName == propertyName).SingleOrDefault();
}

/// <summary>
/// Extension methods for the ModelStatement class.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Gets the model statement by name.
    /// </summary>
    /// <param name="statements">List of model statements</param>
    /// <param name="name">Name of the model</param>
    /// <returns>Returns the model statement</returns>
	public static ModelStatement GetModelStatement(this List<ModelStatement> statements, string name) 
		=> statements.Where(x => x.Name == name).SingleOrDefault();

    /// <summary>
    /// Checks if the model statement contains a property.
    /// </summary>
    /// <param name="columns">List of column statements</param>
    /// <param name="property">Property instance</param>
    /// <returns>Returns true if the model statement contains the property</returns>
	public static bool ContainsProperty(this List<ColumnStatement> columns, Property property) 
		=> columns.Any(y => y.PropertyName == property.Name);

    /// <summary>
    /// Checks if the model statement contains a property with a different column name.
    /// </summary>
    /// <param name="columns">List of column statements</param>
    /// <param name="property">Property instance</param>
    /// <returns>Returns true if the model statement contains the property with a different column name</returns>
	public static bool ColumnNameHasChanged(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name && x.ColumnName != property.ColumnName);

    /// <summary>
    /// Checks if the model statement contains a property with different options.
    /// </summary>
    /// <param name="columns">List of column statements</param>
    /// <param name="property">Property instance</param>
    /// <returns>Returns true if the model statement contains the property with different options</returns>
	public static bool PropertyOptionsHaveChanged(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name 
		&& x.PropertyOptions.RemoveUnique() != ScriptBuilder
			.HandlePropertyOptions(property, Operation.Create)
			.RemoveFormatting()
			.RemoveUnique()
			.Substring(property.ColumnName.Length + 1));

    /// <summary>
    /// Checks if the model statement contains a property that became unique.
    /// </summary>
    /// <param name="columns">List of column statements</param>
    /// <param name="property">Property instance</param>
    /// <returns>Returns true if the model statement contains a property that became unique</returns>
	public static bool ColumnBecameUnique(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name 
			&& !x.PropertyOptions.Contains("UNIQUE") 
			&& ScriptBuilder.HandlePropertyOptions(property, Operation.Create).Contains("UNIQUE"));

    /// <summary>
    /// Checks if the model statement contains a property that lost the unique option.
    /// </summary>
    /// <param name="columns">List of column statements</param>
    /// <param name="property">Property instance</param>
    /// <returns>Returns true if the model statement contains a property that lost the unique option</returns>
	public static bool ColumnLostUnique(this List<ColumnStatement> columns, Property property)
		=> columns.Any(x => x.PropertyName == property.Name 
		            && x.PropertyOptions.Contains("UNIQUE") 
		            && !ScriptBuilder.HandlePropertyOptions(property, Operation.Create).Contains("UNIQUE"));
}

