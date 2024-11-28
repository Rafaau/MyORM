namespace MyORM;

/// <summary>
/// Abstract class that represents an access layer.
/// </summary>
public abstract class AccessLayer
{
    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the connection string.
    /// </summary>
	public abstract string ConnectionString { get; }

    /// <summary>
    /// Gets the options.
    /// </summary>
	public virtual Options Options { get; } = new();
}
