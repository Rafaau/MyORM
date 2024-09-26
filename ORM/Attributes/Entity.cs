namespace MyORM.Attributes;

public class Entity : Attribute
{
	public string Name { get; set; }

	public Entity()
	{
	}

	public Entity(string name)
	{
		Name = name;
	}
}