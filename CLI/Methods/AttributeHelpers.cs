using ORM.Attributes;
using System.Reflection;

namespace CLI.Methods;

internal class AttributeHelpers
{
	internal static void GetPropsByAttribute(Type attributeType)
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies();

		foreach (var assembly in assemblies)
		{
			foreach (var type in assembly.GetTypes())
			{
				var attributes = type.GetCustomAttribute(attributeType, false);
				if (attributes is not null)
				{
					Console.WriteLine("Found");
                }
			}
		}
	}
}

