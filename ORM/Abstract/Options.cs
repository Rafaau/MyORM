using MyORM.Methods;
using System.Configuration;

namespace MyORM;

public class Options
{
	public Database Database { get; set; } = Database.MySQL;
	public string EntitiesAssembly { get; set; } = string.Empty;
	public string MigrationsAssembly { get; set; } = string.Empty;
	public bool KeepConnectionOpen { get; set; } = false;

	public string GetEntitiesAssembly()
	{
		return Directory.GetParent(Directory.GetCurrentDirectory())!.FullName + "\\" + EntitiesAssembly + "\\obj";
	}

	public string GetMigrationsAssembly()
	{
		return Directory.GetParent(Directory.GetCurrentDirectory())!.FullName + "\\" + MigrationsAssembly + "\\obj";
	}

	public string GetMigrationsMainDirectory()
	{
		return Directory.GetParent(Directory.GetCurrentDirectory())!.FullName + "\\" + MigrationsAssembly;
	}
}
