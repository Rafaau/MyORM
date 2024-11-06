namespace MyORM;

public abstract class AbstractMigration
{
	public abstract string GetDescription();
	public abstract void Up(DbHandler dbHandler);
	public abstract void Down(DbHandler dbHandler);
}
