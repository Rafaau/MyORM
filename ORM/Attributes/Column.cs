namespace ORM;

public class Column : Attribute
{
	private string Name { get; set; }

	public Column()
	{
    }

	public Column(string name)
	{
		Name = name;
	}
}

public sealed class PrimaryGeneratedColumn : Column
{
    public PrimaryGeneratedColumn()
    {
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class OneToOne<T> : Column where T : class
{
    public Type RelationModel { get; set; }

    public OneToOne()
    {
        RelationModel = typeof(T);
    }
}
