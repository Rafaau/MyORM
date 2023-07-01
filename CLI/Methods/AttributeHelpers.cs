using ORM.Attributes;
using System.Reflection;

namespace CLI.Methods;

internal class AttributeHelpers
{
	internal class ClassProps
	{
		public string ClassName { get; set; }
		public Dictionary<string, object> AttributeProps { get; set; } = new Dictionary<string, object>();
		public List<FieldProps> FieldProps { get; set; } = new List<FieldProps>();
	}

	internal class FieldProps
	{
		public string FieldName { get; set; }
		public Type FieldType { get; set; }
		public object? FieldValue { get; set; }
		public Dictionary<string, object>? AttributeProps { get; set; }
	}

	internal static List<ClassProps> GetPropsByAttribute(Type attributeType)
	{
		List<ClassProps> props = new List<ClassProps>();
		string currentDirectory = Directory.GetCurrentDirectory() + "\\obj";

        foreach (var file in Directory.EnumerateFiles(currentDirectory, "*.dll", SearchOption.AllDirectories))
		{
			try
			{
				var assembly = Assembly.LoadFrom(file);

				foreach (var type in assembly.GetTypes())
				{
					var attribute = type.GetCustomAttribute(attributeType, true);
					if (attribute != null)
					{
						props.Add(new ClassProps() { ClassName = type.Name });

                        foreach (var property in attribute.GetType().GetProperties())
						{
                            props.Last().AttributeProps.Add(property.Name, property.GetValue(attribute));
						}

						var instance = Activator.CreateInstance(type);
						foreach (var property in type.GetProperties())
						{
                            Console.WriteLine("props?");
                            props.Last().FieldProps.Add(new FieldProps() { 
								FieldName = property.Name,
								FieldType = property.PropertyType,
								FieldValue = property.GetValue(instance),
							});
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

