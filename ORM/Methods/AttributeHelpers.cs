using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Reflection;

namespace MyORM.Methods;

/// <summary>
/// Class that contains helper methods for attributes.
/// </summary>
public class AttributeHelpers
{
    /// <summary>
    /// Gets the path of the assembly.
    /// </summary>
    public static string AssemblyPath
	{
		get
		{
			#if DEBUG
				return ConfigurationManager.AppSettings["AssemblyPath"];
			#else
				return Directory.GetCurrentDirectory() + "\\obj";
			#endif
		}
	}

    /// <summary>
    /// Class that contains the properties of a class.
    /// </summary>
    public class ClassProps
	{
        /// <summary>
        /// Gets or sets the class name.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the attribute properties.
        /// </summary>
        public Dictionary<string, object> AttributeProps { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the properties of the class.
        /// </summary>
        public List<Property> Properties { get; set; } = new List<Property>();

        /// <summary>
        /// Gets or sets the methods of the class.
        /// </summary>
        public List<MethodInfo> Methods { get; set; } = new List<MethodInfo>();

        /// <summary>
        /// Gets or sets the instance of the class.
        /// </summary>
        public object? Instance { get; set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string TableName
		{
			get
			{
				var name = AttributeProps.Where(x => x.Key == "Name");
				return name != null ? name.First().Value.ToString() : ClassName + "s";
			}
		}

        /// <summary>
        /// Gets the name of the primary key column.
        /// </summary>
        public string PrimaryKeyColumnName
		{
            get
			{
                var key = Properties.Where(x => x.Attributes.Any(y => y.Name == "PrimaryGeneratedColumn"));
                return key != null ? key.First().ColumnName : "Id";
            }
        }
	}

    /// <summary>
    /// Class that represents a single property of a class.
    /// </summary>
    public class Property
	{
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the attributes of the property.
        /// </summary>
        public List<Type> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute properties of the property.
        /// </summary>
        public Dictionary<string, object>? AttributeProps { get; set; }

        /// <summary>
        /// Gets or sets the parent class of the property.
        /// </summary>
        public ClassProps ParentClass { get; set; }

        /// <summary>
        /// Gets the name of the column in the table.
        /// </summary>
        public string ColumnName
		{
			get
			{
				string attr = Attributes.Select(x => x.Name).SingleOrDefault();
				if (attr.Contains("OneToOne")
					|| attr.Contains("ManyToOne"))
					return $"{Name}Id";
				else
					return (string)AttributeProps.GetAttributePropertyValue("Name") ?? Name;
			}
		}

        /// <summary>
        /// Gets the related class of the property.
        /// </summary>
        public ClassProps RelatedClass
		{
			get
			{
				return GetPropsByModel(Type);
			}
		}
	}

    /// <summary>
    /// Class that represents a single method of a class.
    /// </summary>
    public class Method
	{
        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the return type of the method.
        /// </summary>
        public Type ReturnType { get; set; } = typeof(void);

        /// <summary>
        /// Gets or sets the parameter types of the method.
        /// </summary>
        public List<Type> ParameterTypes { get; set; } = new();
	}

    /// <summary>
    /// Gets the properties of a class by an attribute.
    /// </summary>
    /// <param name="attributeType">Type of the attribute</param>
    /// <param name="assemblyPath">Assembly path</param>
    /// <returns>Returns a list of class properties</returns>
    public static List<ClassProps> GetPropsByAttribute(Type attributeType, string assemblyPath = "")
	{
		List<ClassProps> props = new List<ClassProps>();


		if (assemblyPath == "")
			assemblyPath = AssemblyPath;

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
							foreach (var attr in property.GetCustomAttributes(true))
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
								Attributes = property.GetAttributes(),
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

    /// <summary>
    /// Gets the properties of a class by the model.
    /// </summary>
    /// <param name="model">Type of the class</param>
    /// <returns>Returns the class properties</returns>
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
					Attributes = property.GetAttributes(),
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

/// <summary>
/// Extension methods for the AttributeHelpers class.
/// </summary>
public static class HelpersExtensions
{
    /// <summary>
    /// Gets the relationship of a property.
    /// </summary>
    /// <param name="properties">List of properties</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Returns the relationship of the property</returns>
    public static Relationship GetRelationship(this List<AttributeHelpers.Property> properties, string propertyName) 
		=> (Relationship)properties
			.Find(x => x.Name == propertyName).AttributeProps
			.FirstOrDefault(x => x.Key == "Relationship").Value;

    /// <summary>
    /// Gets the value of an attribute property.
    /// </summary>
    /// <param name="attributeProps">Dictionary of attribute properties</param>
    /// <param name="attributeProperty">Name of the attribute property</param>
    /// <returns>Returns the value of the attribute property</returns>
    public static object? GetAttributePropertyValue(this Dictionary<string, object> attributeProps, string attributeProperty)
	{
		var property = attributeProps.FirstOrDefault(x => x.Key == attributeProperty);

		if (property.Key == null
			|| property.Value == null
			|| property.Value.ToString().IsNullOrEmpty())
			return null;

		return property.Value;
	}

    /// <summary>
    /// Gets the relationship of a property.
    /// </summary>
    /// <param name="property">Property instance</param>
    /// <returns>Returns the relationship of the property</returns>
    public static MyORM.Relationship GetRelationship(this AttributeHelpers.Property property)
	{
		var relationshipAttr = property.AttributeProps.FirstOrDefault(x => x.Key == "Relationship").Value;
		return relationshipAttr != null ? (Relationship)relationshipAttr : Relationship.Mandatory;
	}

    /// <summary>
    /// Checks if a property has a relationship.
    /// </summary>
    /// <param name="property">Property instance</param>
    /// <param name="relationship">Relationship type</param>
    /// <returns>Returns true if the property has the relationship, otherwise false</returns>
    public static bool HasRelationship(this AttributeHelpers.Property property, Relationship relationship)
		=> property.GetRelationship() == relationship;

    /// <summary>
    /// Checks if a property has a cascade option.
    /// </summary>
    /// <param name="property">Property instance</param>
    /// <returns>Returns true if the property has the cascade option, otherwise false</returns>
    public static bool HasCascadeOption(this AttributeHelpers.Property property)
	{
		bool cascade = false;

		var baseProps = AttributeHelpers.GetPropsByModel(property.Type);

		var cascadeAttr = baseProps.Properties
			.Find(x => x.Type.Name == property.ParentClass.ClassName)?.AttributeProps?
			.FirstOrDefault(x => x.Key == "Cascade").Value;

		if (cascadeAttr != null)
		{
			cascade = (bool)cascadeAttr;
		}

		return cascade;
	}

    /// <summary>
    /// Gets the value of a property.
    /// </summary>
    /// <param name="property">Property instance</param>
    /// <param name="model">Model instance</param>
    /// <returns>Returns the value of the property</returns>
    public static object GetValue(this AttributeHelpers.Property property, object model)
		=> model.GetType().GetProperty(property.Name).GetValue(model);

    /// <summary>
    /// Gets the column name of a property.
    /// </summary>
    /// <param name="props">Class properties</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Returns the column name of the property</returns>
    public static string GetColumnNameByProperty(this AttributeHelpers.ClassProps props, string propertyName)
	{
        var property = props.Properties.Find(x => x.Name == propertyName);
		return property.ColumnName;
    }

    /// <summary>
    /// Gets the properties of a class by the model.
    /// </summary>
    /// <param name="properties">List of properties</param>
    /// <param name="attributes">List of attributes</param>
    /// <returns>Returns the properties list except the ones with the specified attributes</returns>
    public static IEnumerable<AttributeHelpers.Property> ExceptAttributes(this List<AttributeHelpers.Property> properties, params string[] attributes)
	{
        foreach (var attr in attributes)
        {
            foreach (var prop in properties)
            {
                if (!prop.Attributes.Any(x => x.FullName.Contains(attr)))
                    yield return prop;
            }
        }
    }
}