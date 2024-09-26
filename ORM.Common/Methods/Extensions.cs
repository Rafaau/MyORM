using System.Reflection;

namespace MyORM.Common.Methods;

public static class Extensions
{
	public static List<Type> GetAttributes(this MemberInfo type) => type.GetCustomAttributes().ToArray().Select(x => x.GetType()).ToList();
}

