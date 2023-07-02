using ORM.Abstract;
using ORM.Attributes;

namespace Test;

[DataAccessLayer]
public class AppAccessLayer : AccessLayer
{
	public override string Name => "App";
	public override string ConnectionString => "Server=localhost;Database=orm;User Id=root;Password=password;";
}
