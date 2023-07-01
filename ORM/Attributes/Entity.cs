using System;

namespace ORM;

public class Entity : Attribute
{
	public string Name { get; set; }

	public Entity(string name)
	{
		Name = name;
	}
}