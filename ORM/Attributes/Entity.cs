using System;

namespace ORM;

public class Entity : Attribute
{
	private string Name { get; set; }

	public Entity(string name)
	{
		Name = name;
	}
}