namespace MyORM.Projectioner.Methods;

/// <summary>
/// Helper class for projection.
/// </summary>
public static class ProjectionHelper
{
    /// <summary>
    /// Projects the source object to the instance object.
    /// </summary>
    /// <typeparam name="T">The type of the object to project</typeparam>
    /// <param name="source">Source object to project</param>
    /// <param name="instance">Instance object to project to</param>
    /// <returns>Returns the projected object</returns>
    public static T ToProjection<T>(this object source, T instance = null) where T : class, new()
	{
		var project = instance == null ? new T() : instance;
		var sourceProperties = source.GetType().GetProperties();
		var projectProperties = project.GetType().GetProperties();

		foreach (var pProp in projectProperties)
		{
			var sProp = sourceProperties.FirstOrDefault(sp => sp.Name == pProp.Name);

			if (sProp != null)
			{
				if (!pProp.GetType().IsValueType && sProp.PropertyType != pProp.PropertyType)
				{
					var pPropInstance = Activator.CreateInstance(pProp.PropertyType);
					var proj = sProp.GetValue(source, null).ToProjection(pPropInstance);
					pProp.SetValue(project, proj, null);
					continue;
				}

				pProp.SetValue(project, sProp.GetValue(source, null), null);
			}
		}

		return project;
	}
}
