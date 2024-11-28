namespace MyORM.Attributes;

/// <summary>
/// Attribute that represents a column in a table.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class Column : Attribute
{
    /// <summary>
    /// Gets or sets the name of the column in the table.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default value of the column.
    /// </summary>
    public object DefaultValue { get; set; } = null;

    /// <summary>
    /// Gets or sets if the column can be null.
    /// </summary>
    public bool Nullable { get; set; } = true;

    /// <summary>
    /// Gets or sets if the column is unique.
    /// </summary>
    public bool Unique { get; set; } = false;

    /// <summary>
    /// Constructor for the Column attribute.
    /// </summary>
    public Column() { }

    /// <summary>
    /// Constructor for the Column attribute.
    /// </summary>
    /// <param name="name">Name of the column in the table</param>
    /// <param name="defaultValue">Default value of the column</param>
    /// <param name="nullable">If the column can be null</param>
    /// <param name="unique">If the column is unique</param>
    public Column(string name = null, object defaultValue = null, bool nullable = true, bool unique = false)
	{
		Name = name;
		DefaultValue = defaultValue;
		Nullable = nullable;
		Unique = unique;
	}
}

/// <summary>
/// Attribute that represents a primary generated column in a table.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PrimaryGeneratedColumn : Column
{
    /// <summary>
    /// Constructor for the PrimaryGeneratedColumn attribute.
    /// </summary>
    public PrimaryGeneratedColumn() { }

    /// <summary>
    /// Constructor for the PrimaryGeneratedColumn attribute.
    /// </summary>
    /// <param name="name">Name of the column in the table</param>
    public PrimaryGeneratedColumn(string name) : base(name) { }
}

/// <summary>
/// Attribute that represents a one-to-one relationship in a table.
/// </summary>
/// <typeparam name="T">Type of the relation model</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OneToOne<T> : Column where T : class
{
    /// <summary>
    /// Gets or sets the relation model.
    /// </summary>
	public Type RelationModel { get; set; } = null;

    /// <summary>
    /// Gets or sets the relationship type.
    /// </summary>
	public Relationship Relationship { get; set; } = Relationship.Mandatory;

    /// <summary>
    /// Gets or sets if the cascade is enabled.
    /// </summary>
	public bool Cascade { get; set; } = false;

    /// <summary>
    /// Constructor for the OneToOne attribute.
    /// </summary>
	public OneToOne() { }

    /// <summary>
    /// Constructor for the OneToOne attribute.
    /// </summary>
    /// <param name="relationship">Relationship type</param>
    /// <param name="cascade">If the cascade is enabled</param>
	public OneToOne(Relationship relationship = Relationship.Mandatory, bool cascade = false)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
		Cascade = cascade;
	}
}

/// <summary>
/// Attribute that represents a one-to-many relationship in a table.
/// </summary>
/// <typeparam name="T">Type of the relation model</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OneToMany<T> : Column where T : class
{
    /// <summary>
    /// Gets or sets the relation model.
    /// </summary>
	public Type RelationModel { get; set; } = null;

    /// <summary>
    /// Gets or sets the relationship type.
    /// </summary>
	public Relationship Relationship { get; set; } = Relationship.Optional;

    /// <summary>
    /// Constructor for the OneToMany attribute.
    /// </summary>
	public OneToMany() { }

    /// <summary>
    /// Constructor for the OneToMany attribute.
    /// </summary>
    /// <param name="relationship">Relationship type</param>
	public OneToMany(Relationship relationship = Relationship.Optional)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}

/// <summary>
/// Attribute that represents a many-to-one relationship in a table.
/// </summary>
/// <typeparam name="T">Type of the relation model</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ManyToOne<T> : Column where T : class
{
    /// <summary>
    /// Gets or sets the relation model.
    /// </summary>
	public Type RelationModel { get; set; } = null;

    /// <summary>
    /// Gets or sets the relationship type.
    /// </summary>
	public Relationship Relationship { get; set; } = Relationship.Mandatory;

    /// <summary>
    /// Constructor for the ManyToOne attribute.
    /// </summary>
    public ManyToOne() { }

    /// <summary>
    /// Constructor for the ManyToOne attribute.
    /// </summary>
    /// <param name="relationship">Relationship type</param>
    public ManyToOne(Relationship relationship = Relationship.Mandatory)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}

/// <summary>
/// Attribute that represents a many-to-many relationship in a table.
/// </summary>
/// <typeparam name="T">Type of the relation model</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ManyToMany<T> : Column where T : class
{
    /// <summary>
    /// Gets or sets the relation model.
    /// </summary>
	public Type RelationModel { get; set; } = null;

    /// <summary>
    /// Gets or sets the relationship type.
    /// </summary>
	public Relationship Relationship { get; set; } = Relationship.Optional;

    /// <summary>
    /// Constructor for the ManyToMany attribute.
    /// </summary>
	public ManyToMany() { }

    /// <summary>
    /// Constructor for the ManyToMany attribute.
    /// </summary>
    /// <param name="relationship">Relationship type</param>
	public ManyToMany(Relationship relationship = Relationship.Optional)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}
