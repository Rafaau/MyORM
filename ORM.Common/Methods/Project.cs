using System.Diagnostics;

namespace MyORM.Common.Methods;

public static class Project
{
	public static void Build()
	{
		string csproj = GetCsproj();

		ProcessStartInfo psi = new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $"build {csproj}",
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		var process = Process.Start(psi);
		string output = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		//if (process.ExitCode != 0)
		//    throw new Exception(error);
	}

	public static string GetCsproj()
	{
		string currentDirectory = Directory.GetCurrentDirectory();

		while (true)
		{
			var csprojFiles = Directory.GetFiles(currentDirectory, "*.csproj");

			if (csprojFiles.Length > 0)
				return csprojFiles[0];

			var parentDirectory = Directory.GetParent(currentDirectory);
			if (parentDirectory == null)
				throw new FileNotFoundException();

			currentDirectory = parentDirectory.FullName;
		}
	}
}


