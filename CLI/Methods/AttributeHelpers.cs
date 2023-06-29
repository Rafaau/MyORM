using ORM.Attributes;
using System.Reflection;

namespace CLI.Methods;

internal class AttributeHelpers
{
	internal static Dictionary<string, object> GetPropsByAttribute(Type attributeType)
	{
		Dictionary<string, object> props = new();
		string currentDirectory = Directory.GetCurrentDirectory() + "\\obj";

        foreach (var file in Directory.EnumerateFiles(currentDirectory, "*.dll", SearchOption.AllDirectories))
		{
			try
			{
				var assembly = Assembly.LoadFrom(file);

				foreach (var type in assembly.GetTypes())
				{
					var attributes = type.GetCustomAttributes(attributeType, true);
					if (attributes.Any())
					{
						var instance = Activator.CreateInstance(type);
						foreach (var property in type.GetProperties())
						{
							props.Add(property.Name, property.GetValue(instance));
                        }
					}
				}

				break;
			}
			catch (BadImageFormatException)
			{
				// Ignore native DLLs and other files that aren't .NET assemblies.
			}
		}

		return props;
	}
}

