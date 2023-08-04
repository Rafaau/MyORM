namespace ORM.Models;
public class ModelStatement
{
    public string Name { get; set; }
    public string TableName { get; set; }
    public List<ColumnStatement> Columns { get; set; }

    public ModelStatement(string name, string tableName, List<ColumnStatement> columns)
    {
        Name = name;
        TableName = tableName;
        Columns = columns;
    }
}

