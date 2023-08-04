using System.Reflection;

namespace CLI.Methods;

internal class AttributeHelpers
{
	internal class ClassProps
	{
		public string ClassName { get; set; }
		public Dictionary<string, object> AttributeProps { get; set; } = new Dictionary<string, object>();
		public List<Property> Properties { get; set; } = new List<Property>();
		public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
		public object? Instance { get; set; }
	}

	internal class Property
	{
		public string Name { get; set; }
		public Type Type { get; set; }
		public object? Value { get; set; }
		public List<Type> Attributes { get; set; }
		public Dictionary<string, object>? AttributeProps { get; set; }
	}

	internal class Method
	{
		public string Name { get; set; }
		public Type ReturnType { get; set; }
		public List<Type> ParameterTypes { get; set; }
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
                            props.Last().Properties.Add(new Property() { 
								Name = property.Name,
								Type = property.PropertyType,
								Value = property.GetValue(instance),
								Attributes = property
									.GetCustomAttributes()
									.ToArray()
									.Select(x => x.GetType())
									.ToList(),
							});
                        }

						foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
						{
                            props.Last().Methods.Add(method);
						}

						props.Last().Instance = instance;
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

