namespace MyORM;

/// <summary>
/// Class that represents the options for the AccessLayer.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets or sets the database type.
    /// </summary>
    public Database Database { get; set; } = Database.MySQL;

    /// <summary>
    /// Gets or sets the entities assembly.
    /// </summary>
    public string EntitiesAssembly { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the migrations assembly.
    /// </summary>
    public string MigrationsAssembly { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets if the connection should be kept open.
    /// </summary>
    public bool KeepConnectionOpen { get; set; } = false;

    /// <summary>
    /// Gets the entities assembly path.
    /// </summary>
    /// <returns>Returns the entities assembly path</returns>
	public string GetEntitiesAssembly()
	{
		return Directory.GetParent(Directory.GetCurrentDirectory())!.FullName + "\\" + EntitiesAssembly + "\\obj";
	}

    /// <summary>
    /// Gets the migrations assembly path.
    /// </summary>
    /// <returns>Returns the migrations assembly path</returns>
	public string GetMigrationsAssembly()
	{
		return Directory.GetParent(Directory.GetCurrentDirectory())!.FullName + "\\" + MigrationsAssembly + "\\obj";
	}

    /// <summary>
    /// Gets the migrations main directory.
    /// </summary>
    /// <returns>Returns the migrations main directory</returns>
	public string GetMigrationsMainDirectory()
	{
		return Directory.GetParent(Directory.GetCurrentDirectory())!.FullName + "\\" + MigrationsAssembly;
	}
}
