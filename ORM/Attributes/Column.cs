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
	public Type RelationModel { get; set; }
	public Relationship Relationship { get; set; }
	public bool Cascade { get; set; }

	public OneToOne()
	{
		RelationModel = typeof(T);
		Relationship = Relationship.Mandatory;
		Cascade = false;
	}

	public OneToOne(Relationship relationship)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
		Cascade = false;
	}

	public OneToOne(Relationship relationship, bool cascade)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
		Cascade = cascade;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class OneToMany<T> : Column where T : class
{
	public Type RelationModel { get; set; }
	public Relationship Relationship { get; set; }

	public OneToMany()
	{
		RelationModel = typeof(T);
		Relationship = Relationship.Optional;
	}

	public OneToMany(Relationship relationship)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ManyToOne<T> : Column where T : class
{
	public Type RelationModel { get; set; }
	public Relationship Relationship { get; set; }

	public ManyToOne()
	{
		RelationModel = typeof(T);
		Relationship = Relationship.Mandatory;
	}

	public ManyToOne(Relationship relationship)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ManyToMany<T> : Column where T : class
{
	public Type RelationModel { get; set; }
	public Relationship Relationship { get; set; }

	public ManyToMany()
	{
		RelationModel = typeof(T);
		Relationship = Relationship.Optional;
	}

	public ManyToMany(Relationship relationship)
	{
		RelationModel = typeof(T);
		Relationship = relationship;
	}
}
