namespace MyORM.Models;

public class ColumnStatement
{
	public string Name { get; set; }
	public string Type { get; set; }

	public ColumnStatement(string name, string type)
	{
		Name = name;
		Type = type;
	}
}

