namespace MyORM.Projectioner.Methods;

public static class ProjectionHelper
{
	public static T ToProjection<T>(this object source) where T : class, new()
	{
		var project = new T();
		var sourceProperties = source.GetType().GetProperties();
		var projectProperties = project.GetType().GetProperties();

		foreach (var pProp in projectProperties)
		{
			var sProp = sourceProperties.FirstOrDefault(sp => sp.Name == pProp.Name);
			if (sProp != null)
			{
				pProp.SetValue(project, sProp.GetValue(source, null), null);
			}
		}

		return project;
	}
}
