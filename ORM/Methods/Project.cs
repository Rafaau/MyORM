using System.Diagnostics;

namespace MyORM.Methods;

/// <summary>
/// Class for project build.
/// </summary>
public static class Project
{
    /// <summary>
    /// Builds the project.
    /// </summary>
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
	}

    /// <summary>
    /// Gets the csproj file path.
    /// </summary>
    /// <returns>Returns the csproj file path</returns>
    /// <exception cref="FileNotFoundException">Exception that is thrown when the csproj file is not found.</exception>
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


