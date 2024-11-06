using MyORM;

namespace Test;

[DataAccessLayer]
public class AppAccessLayer : AccessLayer
{
	public override string Name => "App";

	// MySQL
	//public override string ConnectionString => "Server=localhost;Database=orm;User Id=root;Password=password;";

	// PostgreSQL
	//public override string ConnectionString => "Server=localhost;Database=postgres;User Id=postgres;Password=password;";

	// Microsoft SQL Server
	public override string ConnectionString => "Server=127.0.0.1;Database=master;User Id=sa;Password=Password123!;Encrypt=Yes;TrustServerCertificate=Yes;";

	public override Options Options => new()
	{
		Database = Database.MicrosoftSQLServer,
		EntitiesAssembly = "Test",
		MigrationsAssembly = "Test",
		KeepConnectionOpen = true
	};
}
