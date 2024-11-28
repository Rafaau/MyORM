namespace MyORM.Models;

/// <summary>
/// Class that represents a database statement.
/// </summary>
public class DBStatement
{
    /// <summary>
    /// Gets or sets the list of model statements.
    /// </summary>
    public List<ModelStatement> Models { get; set; } = new();
}

