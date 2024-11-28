using MyORM.Models;

namespace MyORM;

/// <summary>
/// Abstract class that represents a snapshot file.
/// </summary>
public abstract class AbstractSnapshot
{
    /// <summary>
    /// Gets the metadata of the snapshot file.
    /// </summary>
    /// <returns>Returns the metadata of the snapshot file</returns>
    public abstract string GetMetadata();

    /// <summary>
    /// Launches the scripts to create the database from the snapshot.
    /// </summary>
    /// <param name="dbHandler">DbHandler instance</param>
	public abstract void CreateDBFromSnapshot(DbHandler dbHandler);

    /// <summary>
    /// Gets the list of models statements.
    /// </summary>
    /// <returns>Returns the list of models statements</returns>
	public abstract List<ModelStatement> GetModelsStatements();
}
