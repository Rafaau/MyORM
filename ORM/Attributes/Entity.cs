namespace MyORM;

/// <summary>
/// Attribute that represents an entity.
/// </summary>
public class Entity : Attribute
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Constructor for the Entity attribute.
    /// </summary>
    public Entity()
	{
	}

    /// <summary>
    /// Constructor for the Entity attribute.
    /// </summary>
    /// <param name="name">Name of the table in the database</param>
	public Entity(string name)
	{
		Name = name;
	}
}