namespace ORM.Abstract;

public abstract class AbstractSnapshot
{
	public abstract string GetMetadata();
	public abstract void CreateDBFromSnapshot(Schema schema);
}
