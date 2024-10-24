using System.Reflection;
using static MyORM.Common.Methods.AttributeHelpers;

namespace MyORM.Common.Methods;

public static class Extensions
{
	public static List<Type> GetAttributes(this MemberInfo type) => type.GetCustomAttributes().ToArray().Select(x => x.GetType()).ToList();
	public static object GetPropertyValue(this object obj, string propertyName) => obj.GetType().GetProperty(propertyName).GetValue(obj);
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
	public static Property WithNameAndAttribute(this List<Property> props, string name, string attribute)
		=> props.Find(x => x.Attributes.Any(x => x.Name.Contains(attribute)) && x.Type.Name == name);
	public static bool HasAttribute(this PropertyInfo prop, string attribute) => prop.GetAttributes().Any(x => x.FullName.Contains(attribute));
	public static bool HasAttribute(this Property prop, string attribute) => prop.Attributes.Any(x => x.FullName.Contains(attribute));
}

