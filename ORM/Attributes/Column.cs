namespace MyORM.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class Column : Attribute
{
	public string Name { get; set; } = string.Empty;
	public object DefaultValue { get; set; } = null;
	public bool Nullable { get; set; } = true;
	public bool Unique { get; set; } = false;

	public Column() { }

	public Column(string name = null, object defaultValue = null, bool nullable = true, bool unique = false)
	{
		Name = name;
		DefaultValue = defaultValue;
		Nullable = nullable;
		Unique = unique;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class PrimaryGeneratedColumn : Column
{
	public PrimaryGeneratedColumn() { }

	public PrimaryGeneratedColumn(string name) : base(name) { }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class OneToOne<T> : Column where T : class
{
	public Type RelationModel { get; set; } = null;
	public Relationship Relationship { get; set; } = Relationship.Mandatory;
	public bool Cascade { get; set; } = false;

	public OneToOne() { }

	public OneToOne(Relationship relationship = Relationship.Mandatory, bool cascade = false)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
		Cascade = cascade;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class OneToMany<T> : Column where T : class
{
	public Type RelationModel { get; set; } = null;
	public Relationship Relationship { get; set; } = Relationship.Optional;

	public OneToMany() { }

	public OneToMany(Relationship relationship = Relationship.Optional)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ManyToOne<T> : Column where T : class
{
	public Type RelationModel { get; set; } = null;
	public Relationship Relationship { get; set; } = Relationship.Mandatory;

    public ManyToOne() { }

    public ManyToOne(Relationship relationship = Relationship.Mandatory)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ManyToMany<T> : Column where T : class
{
	public Type RelationModel { get; set; } = null;
	public Relationship Relationship { get; set; } = Relationship.Optional;

	public ManyToMany() { }

	public ManyToMany(Relationship relationship = Relationship.Optional)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}
