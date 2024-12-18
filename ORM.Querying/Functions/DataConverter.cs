﻿using MyORM.Models;
using MyORM.Methods;
using System.Data;
using System.Reflection;
using System.Collections;

namespace MyORM.Querying.Functions;

/// <summary>
/// Class that converts data from a DataTable to a model.
/// </summary>
internal class DataConverter
{
    /// <summary>
    /// List of model statements.
    /// </summary>
    private readonly List<ModelStatement> _statementList;

    /// <summary>
    /// Constructor for the <see cref="DataConverter"/> class.
    /// </summary>
    /// <param name="statementList">List of model statements</param>
    public DataConverter(List<ModelStatement> statementList)
    {
        _statementList = statementList;
    }

    /// <summary>
    /// Maps the data from a DataTable to a model.
    /// </summary>
    /// <typeparam name="S">Type of the model</typeparam>
    /// <param name="table">Table to map the data</param>
    /// <param name="instance">Instance of the model</param>
    /// <param name="parent">Parent object</param>
    /// <returns>Returns the mapped data</returns>
    internal IEnumerable<S> MapData<S>(DataTable table, S instance = null, object parent = null) where S : class, new()
	{
		ModelStatement statement;
		List<S> result = new();
		S obj;
		PropertyInfo[] props;

		if (instance is null)
		{
			statement = _statementList.GetModelStatement(typeof(S).Name);
			props = typeof(S).GetProperties();
		}
		else
		{
			statement = _statementList.GetModelStatement(instance.GetType().Name);
			props = instance.GetType().GetProperties();
		}

		foreach (DataRow row in table.Rows)
		{
			obj = instance is null 
				? new S() 
				: Activator.CreateInstance(instance.GetType()) as S;

			string pkName = statement.GetPrimaryKeyPropertyName();

			if (parent is not null)
			{
				ModelStatement parentStatement = _statementList.GetModelStatement(parent.GetType().Name);

				if (!table.Columns.Contains($"{parentStatement.TableName}.{parentStatement.GetPrimaryKeyColumnName()}"))
					continue;

                int parentPK = (int)row[$"{parentStatement.TableName}.{parentStatement.GetPrimaryKeyColumnName()}"];

				if (!parentPK.Equals((int)parent.GetPropertyValue(parentStatement.GetPrimaryKeyPropertyName())))
					continue;
			}

			foreach (var prop in props.ExceptAttributes(["ManyToMany"]))
			{
				if (instance is not null && prop.Name == parent?.GetType().Name)
					continue;

				string columnName = statement.GetColumnName(prop.Name);

				if (table.Columns.Contains($"{statement.TableName}.{columnName}")
					&& row[$"{statement.TableName}.{columnName}"] != DBNull.Value)
				{
					if (IsSimpleType(prop.PropertyType))
					{
						var value = Convert.ChangeType(row[$"{statement.TableName}.{columnName}"], prop.PropertyType);
						prop.SetValue(obj, value);
					}
					else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
					{
						object nestedObj = GetNestedObject(table, row, prop, statement, obj);

						prop.SetValue(obj, nestedObj);
						nestedObj?.GetType().GetProperty(obj.GetType().Name).SetValue(nestedObj, obj);
					}
				}
				else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
				{
					IList nestedObjectList = GetNestedObjectList(table, row, prop, pkName, obj);

					prop.SetValue(obj, nestedObjectList);
				}
			}

			// Check if the object is already in the list
			if (result.Any(x => x.Equals(obj)))
				continue;

			result.Add(obj);
		}

		return result;
	}

    /// <summary>
    /// Checks if the type is a simple type.
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>Returns true if the type is a simple type, otherwise false</returns>
    private bool IsSimpleType(Type type)
		=> type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal);

    /// <summary>
    /// Gets the nested object.
    /// </summary>
    /// <typeparam name="S">Type of the model</typeparam>
    /// <param name="table">Table to get the data</param>
    /// <param name="row">Row to get the data</param>
    /// <param name="property">Property to get the data</param>
    /// <param name="statement">Statement of the model</param>
    /// <param name="parentObj">Parent object</param>
    /// <returns>Returns the nested object</returns>
    private object GetNestedObject<S>(DataTable table, DataRow row, PropertyInfo property, ModelStatement statement, S parentObj) where S : class, new()
	{
		object nestedInstance = Activator.CreateInstance(property.PropertyType);
		ColumnStatement nestedColumn = statement.GetColumn(property.Name);

		IEnumerable<object> mappedObjects = MapData(table, nestedInstance, parentObj);
		object nestedObj = mappedObjects.FirstOrDefault(
			x => x.GetPropertyValue(statement.GetPrimaryKeyPropertyName())
				  .Equals(row[$"{statement.TableName}.{nestedColumn.ColumnName}"])
			);

		return nestedObj;
	}

    /// <summary>
    /// Gets the nested object list.
    /// </summary>
    /// <typeparam name="S">Type of the model</typeparam>
    /// <param name="table">Table to get the data</param>
    /// <param name="row">Row to get the data</param>
    /// <param name="property">Property to get the data</param>
    /// <param name="pkName">Primary key name</param>
    /// <param name="parentObj">Parent object</param>
    /// <returns>Returns the nested object list</returns>
    private IList GetNestedObjectList<S>(DataTable table, DataRow row, PropertyInfo property, string pkName, S parentObj) where S : class, new()
	{
		Type itemType = property.PropertyType.GetGenericArguments()[0];
		var nestedInstance = Activator.CreateInstance(itemType);
		var nestedList = MapData(table, nestedInstance, parentObj);
		object relListInstance = Activator.CreateInstance(property.PropertyType);
		IList relList = (IList)relListInstance;

		foreach (var nestedItem in nestedList)
		{
			int objId = (int)parentObj.GetType().GetProperty(pkName).GetValue(parentObj);
			var nestedStatement = _statementList.FirstOrDefault(x => x.Name == itemType.Name);
			var nestedColumn = nestedStatement.GetColumn(parentObj.GetType().Name).ColumnName;
			var relId = row[$"{nestedStatement.TableName}.{nestedColumn}"];

			// Check if the nested object foreign key matches the parent object primary key
			if (relId != DBNull.Value && (int)relId == objId)
			{
				nestedItem.GetType().GetProperty(parentObj.GetType().Name).SetValue(nestedItem, parentObj);

                if (nestedItem.GetPropertyValue(parentObj.GetType().Name).GetPropertyValue(pkName).Equals(parentObj.GetPropertyValue(pkName)))
					relList.Add(nestedItem);
			}
		}

		return relList;
	}
}
