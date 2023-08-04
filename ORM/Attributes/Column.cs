namespace ORM.Attributes;

public class PrimaryGeneratedColumn : Attribute
{
	public PrimaryGeneratedColumn()
	{
	}
}

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
