using MyORM.Enums;

namespace MyORM.Attributes;

public class Column : Attribute
{
	private string Name { get; set; }

	public Column() { }

	public Column(string name)
	{
		Name = name;
	}
}

public sealed class PrimaryGeneratedColumn : Column
{
	public PrimaryGeneratedColumn() { }
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
