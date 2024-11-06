namespace MyORM;

public abstract class AccessLayer
{
	public abstract string Name { get; }
	public abstract string ConnectionString { get; }
	public virtual Options Options { get; } = new();
}
