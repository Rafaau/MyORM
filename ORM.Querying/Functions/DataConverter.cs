﻿using MyORM.Models;
using MyORM.Methods;
using System.Data;
using System.Reflection;
using System.Collections;

namespace MyORM.Querying.Functions;

internal class DataConverter
{
	private List<ModelStatement> StatementList { get; set; }

    public DataConverter(List<ModelStatement> statementList)
    {
        StatementList = statementList;
    }

    internal IEnumerable<S> MapData<S>(DataTable table, S instance = null, object parent = null) where S : class, new()
	{
		ModelStatement statement;
		List<S> result = new();
		S obj;
		PropertyInfo[] props;

		if (instance is null)
		{
			statement = StatementList.GetModelStatement(typeof(S).Name);
			props = typeof(S).GetProperties();
		}
		else
		{
			statement = StatementList.GetModelStatement(instance.GetType().Name);
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
				ModelStatement parentStatement = StatementList.GetModelStatement(parent.GetType().Name);

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

	private bool IsSimpleType(Type type)
		=> type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal);

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
			var nestedStatement = StatementList.FirstOrDefault(x => x.Name == itemType.Name);
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
