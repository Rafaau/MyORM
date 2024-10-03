using MyORM.Attributes;
using System.Reflection;

namespace MyORM.Common.Methods;

public class AttributeHelpers
{
	public class ClassProps
	{
		public string ClassName { get; set; }
		public Dictionary<string, object> AttributeProps { get; set; } = new Dictionary<string, object>();
		public List<Property> Properties { get; set; } = new List<Property>();
		public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();
		public object? Instance { get; set; }

		public string TableName
		{
			get
			{
				var name = AttributeProps.Where(x => x.Key == "Name");
				return name != null ? name.First().Value.ToString() : ClassName + "s";
			}
		}
	}

	public class Property
	{
		public string Name { get; set; }
		public Type Type { get; set; }
		public object? Value { get; set; }
		public List<Type> Attributes { get; set; }
		public Dictionary<string, object>? AttributeProps { get; set; }
		public ClassProps ParentClass { get; set; }

		public string ColumnName
		{
			get
			{
				string attr = Attributes.Select(x => x.Name).SingleOrDefault();
				if (attr.Contains("OneToOne") 
					|| attr.Contains("ManyToOne"))
					return $"{Name}Id";
				else
					return Name;
			}
		}
	}

	public class Method
	{
		public string Name { get; set; } = string.Empty;
		public Type ReturnType { get; set; } = typeof(void);
		public List<Type> ParameterTypes { get; set; } = new();
	}

	public static List<ClassProps> GetPropsByAttribute(Type attributeType, string assemblyPath = "")
	{
		List<ClassProps> props = new List<ClassProps>();
		if (assemblyPath == "")
			assemblyPath = Directory.GetCurrentDirectory() + "\\obj";

		foreach (var file in Directory.EnumerateFiles(assemblyPath, "*.dll", SearchOption.AllDirectories))
		{
			try
			{
				var assembly = Assembly.LoadFrom(file);
				var list = assembly.GetTypes();
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
							Dictionary<string, object> attributeProps = new();
							foreach (var attr in property.GetCustomAttributes())
							{
								foreach (var prop in attr.GetType().GetProperties())
								{
									attributeProps.Add(prop.Name, prop.GetValue(attr));
								}
							}

							props.Last().Properties.Add(new Property()
							{
								Name = property.Name,
								Type = property.PropertyType,
								Value = property.GetValue(instance),
								Attributes = property
									.GetCustomAttributes()
									.ToArray()
									.Select(x => x.GetType())
									.ToList(),
								AttributeProps = attributeProps,
								ParentClass = GetPropsByModel(type)
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

	public static ClassProps GetPropsByModel(Type model)
	{
		var props = new ClassProps() { ClassName = model.Name };
		var attribute = model.GetCustomAttribute(typeof(Entity), true);

		if (attribute != null)
		{
			foreach (var property in attribute.GetType().GetProperties())
			{
				props.AttributeProps.Add(property.Name, property.GetValue(attribute));
			}

			var instance = Activator.CreateInstance(model);

			foreach (var property in model.GetProperties())
			{
				Dictionary<string, object> attributeProps = new();
				foreach (var attr in property.GetCustomAttributes())
				{
					foreach (var prop in attr.GetType().GetProperties())
					{
						attributeProps.Add(prop.Name, prop.GetValue(attr));
					}
				}

				props.Properties.Add(new Property()
				{
					Name = property.Name,
					Type = property.PropertyType,
					Value = property.GetValue(instance),
					Attributes = property
						.GetCustomAttributes()
						.ToArray()
						.Select(x => x.GetType())
						.ToList(),
					AttributeProps = attributeProps,
					ParentClass = props,
				});
			}

			foreach (var method in model.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
			{
				props.Methods.Add(method);
			}

			props.Instance = instance;
		}

		return props;
	}
}