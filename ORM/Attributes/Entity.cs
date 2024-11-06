namespace MyORM;

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