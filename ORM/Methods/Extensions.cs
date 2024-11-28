using System.Reflection;
using static MyORM.Methods.AttributeHelpers;

namespace MyORM.Methods;

/// <summary>
/// General extension methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Gets the attributes of a member.
    /// </summary>
    /// <param name="type">MemberInfo instance</param>
    /// <returns>Returns the attributes of the member</returns>
    public static List<Type> GetAttributes(this MemberInfo type) => type.GetCustomAttributes().ToArray().Select(x => x.GetType()).ToList();

    /// <summary>
    /// Gets the value of a property.
    /// </summary>
    /// <param name="obj">Object instance</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Returns the value of the property</returns>
    public static object? GetPropertyValue(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName)?.GetValue(obj);

    /// <summary>
    /// Gets the value of a property.
    /// </summary>
    /// <param name="props">List of properties</param>
    /// <param name="attributes">List of attributes</param>
    /// <returns>Returns the properties that do not have the specified attributes</returns>
    public static IEnumerable<PropertyInfo> ExceptAttributes(this IEnumerable<PropertyInfo> props, string[] attributes) 
	{
		foreach (var attr in attributes)
		{
			foreach (var prop in props)
			{
				if (!prop.GetAttributes().Any(x => x.FullName.Contains(attr)))
					yield return prop;
			}
		}
	}

    /// <summary>
    /// Gets the list of properties that have the specified attributes.
    /// </summary>
    /// <param name="props">List of properties</param>
    /// <param name="attributes">List of attributes</param>
    /// <returns>Returns the properties that have the specified attributes</returns>
    public static IEnumerable<PropertyInfo> WithAttributes(this IEnumerable<PropertyInfo> props, string[] attributes)
	{
		foreach (var attr in attributes)
		{
			foreach (var prop in props)
			{
				if (prop.GetAttributes().Any(x => x.FullName.Contains(attr)))
					yield return prop;
			}
		}
	}

    /// <summary>
    /// Gets the list of properties that have the specified attributes.
    /// </summary>
    /// <param name="props">List of properties</param>
    /// <param name="name">Name of the property</param>
    /// <param name="attribute">Name of the attribute</param>
    /// <returns>Returns the property that has the specified name and attribute</returns>
	public static Property WithNameAndAttribute(this List<Property> props, string name, string attribute)
		=> props.Find(x => x.Attributes.Any(x => x.Name.Contains(attribute)) && x.Type.Name == name);

    /// <summary>
    /// Checks if a property has a specified attribute.
    /// </summary>
    /// <param name="prop">PropertyInfo instance</param>
    /// <param name="attribute">Name of the attribute</param>
    /// <returns>Returns true if the property has the specified attribute</returns>
	public static bool HasAttribute(this PropertyInfo prop, string attribute) => prop.GetAttributes().Any(x => x.FullName.Contains(attribute));

    /// <summary>
    /// Checks if a property has a specified attribute.
    /// </summary>
    /// <param name="prop">Property instance</param>
    /// <param name="attribute">Name of the attribute</param>
    /// <returns>Returns true if the property has the specified attribute</returns>
	public static bool HasAttribute(this Property prop, string attribute) => prop.Attributes.Any(x => x.FullName.Contains(attribute));

    /// <summary>
    /// Checks if a type has a specified attribute.
    /// </summary>
    /// <param name="type">Type instance</param>
    /// <param name="attribute">Name of the attribute</param>
    /// <returns>Returns true if the type has the specified attribute</returns>
	public static bool HasToManyAttribute(this Type type, string attribute, string propertyName)
		=> type.GetProperties().Any(x => x.HasAttribute(attribute) && x.PropertyType.GetGenericArguments()[0].Name == propertyName);
}

