namespace MyORM;

/// <summary>
/// Abstract class that represents a migration file.
/// </summary>
public abstract class AbstractMigration
{
    /// <summary>
    /// Gets the description of the migration.
    /// </summary>
    /// <returns>Returns the description of the migration</returns>
    public abstract string GetDescription();

    /// <summary>
    /// Up method that is called when the migration is applied.
    /// </summary>
    /// <param name="dbHandler">DbHandler instance</param>
	public abstract void Up(DbHandler dbHandler);

    /// <summary>
    /// Down method that is called when the migration is rolled back.
    /// </summary>
    /// <param name="dbHandler">DbHandler instance</param>
	public abstract void Down(DbHandler dbHandler);
}
