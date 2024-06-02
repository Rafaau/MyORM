namespace ORM.Abstract;

public class Options
{
	public string EntitiesAssembly { get; set; } = string.Empty;
	public string MigrationsAssembly { get; set; } = string.Empty;
	public bool KeepConnectionOpen { get; set; } = false;

	public string GetEntitiesAssembly()
	{
		var currentDirectory = Directory.GetCurrentDirectory().Split('\\');
		currentDirectory[^4] = this.EntitiesAssembly;
		return string.Join('\\', currentDirectory.Take(currentDirectory.Length - 3)) + "\\obj";
	}

	public string GetMigrationsAssembly()
	{
		var currentDirectory = Directory.GetCurrentDirectory().Split('\\');
		currentDirectory[^4] = this.MigrationsAssembly;
		return string.Join('\\', currentDirectory.Take(currentDirectory.Length - 3)) + "\\obj";
	}

	public string GetMigrationsMainDirectory()
	{
		var currentDirectory = Directory.GetCurrentDirectory().Split('\\');
		currentDirectory[^4] = this.MigrationsAssembly;
		return string.Join('\\', currentDirectory.Take(currentDirectory.Length - 3));
	}
}
