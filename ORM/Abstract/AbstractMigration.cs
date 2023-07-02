namespace ORM.Abstract;

public abstract class AbstractMigration
{
	public abstract string GetDescription();
	public abstract void Up(Schema schema);
	public abstract void Down(Schema schema);
}
