using System.Data;
using System.Reflection;
using System.Linq.Expressions;
using MyORM.Abstract;
using MyORM.Common.Methods;
using MyORM.Querying.Abstract;
using MyORM.Models;
using MyORM.Attributes;
using MyORM.Enums;
using System.Collections;

namespace MyORM.Querying.Repository;

public class Repository<T> : IRepository<T> where T : class, new()
{
	private Type Model { get; set; }
	private DbHandler DbHandler { get; set; }
	private AttributeHelpers.ClassProps ModelProps { get; set; }
	private List<ModelStatement> StatementList { get; set; }

	private string OrderByColumn { get; set; } = string.Empty;
	private string WhereString { get; set; } = string.Empty;
	private string SelectColumns { get; set; } = string.Empty;
	private List<string> AllColumnsList { get; set; } = new();
	private string AllColumnsString
	{
		get
		{
			return string.Join(", ", AllColumnsList);
		}
	}

	public Repository(DbHandler dbHandler)
	{
		Model = typeof(T);

		DbHandler = dbHandler;
		ModelProps = AttributeHelpers.GetPropsByModel(Model);

		try
		{
			var snapshotProps = AttributeHelpers.GetPropsByAttribute(typeof(Snapshot), dbHandler.AccessLayer.Options.GetMigrationsAssembly()).Last();
			var method = snapshotProps.Methods.Find(x => x.Name == "GetModelsStatements");
			StatementList = (List<ModelStatement>)method!.Invoke(snapshotProps.Instance, null)!;
		}
		catch (Exception e)
		{
			throw new Exception("Error getting model statements from Access Layer", e);
		}
	}

	public int Create(T model)
	{
		return InsertInto(model);
	}

	public IEnumerable<T> Find()
	{
		string join = FindAllRelations(ModelProps, null);
		string selectColumns = SelectColumns.Length > 0 
			? SelectColumns
			: AllColumnsString;

		var sql = $"SELECT {selectColumns} FROM {ModelProps.TableName} {join} {WhereString} {OrderByColumn}";
		var result = DbHandler.Query(sql);
		var data = MapData<T>(result);
		return data;
	}

	public T? FindOne()
	{
		string join = FindAllRelations(ModelProps, null);
		string selectColumns = SelectColumns.Length > 0
			? SelectColumns
			: AllColumnsString;

		var sql = $"SELECT {selectColumns} FROM {ModelProps.TableName} {join} {WhereString} {OrderByColumn} LIMIT 1";
		var result = DbHandler.Query(sql);
		var data = MapData<T>(result);
		return data.FirstOrDefault();
	}

	public void Update(T model)
	{
		var columns = new List<string>();
		var values = new List<string>();

		foreach (var property in model.GetType().GetProperties())
		{
			var columnName = property.Name;
			var columnValue = property.GetValue(model);

			if (columnValue is null)
			{
				continue;
			}

			if (property.GetAttributes().Any(x => x.Name == "PrimaryGeneratedColumn"))
			{
				WhereString = $"WHERE {columnName} = {columnValue}";
			}

			if (columnValue.GetType() == typeof(string))
			{
				columnValue = $"'{columnValue}'";
			}

			columns.Add($"{columnName} = {columnValue}");
		}

		string columnsString = string.Join(", ", columns);
		var sql = $"UPDATE {ModelProps.TableName} SET {columnsString} {WhereString}";
		DbHandler.Execute(sql);
	}

	public void UpdateMany(T model)
	{
		var columns = new List<string>();
		var values = new List<string>();

		foreach (var property in model.GetType().GetProperties())
		{
			var columnName = property.Name;
			var columnValue = property.GetValue(model);

			if (columnValue is null || property.GetAttributes().Any(x => x.Name == "PrimaryGeneratedColumn"))
			{
				continue;
			}

			if (columnValue.GetType() == typeof(string))
			{
				columnValue = $"'{columnValue}'";
			}

			columns.Add($"{columnName} = {columnValue}");
		}

		string columnsString = string.Join(", ", columns);
		var sql = $"UPDATE {ModelProps.TableName} SET {columnsString} {WhereString}";
		DbHandler.Execute(sql);
	}

	public void Delete()
	{
		var sql = $"DELETE FROM {ModelProps.TableName} {WhereString}";
		DbHandler.Execute(sql);
	}

	public void Delete(T model)
	{
		foreach (var property in model.GetType().GetProperties())
		{
			if (property.GetAttributes().Any(x => x.Name == "PrimaryGeneratedColumn"))
			{
				WhereString = $"WHERE {property.Name} = {property.GetValue(model)}";
			}
		}

		var sql = $"DELETE FROM {ModelProps.TableName} {WhereString}";
		DbHandler.Execute(sql);
	}

	public Repository<T> OrderBy(string columnName, string order = "ASC")
	{
		OrderByColumn = Parameters<T>.GetOrderString(columnName, order);
		return this;
	}

	public Repository<T> Where(Expression<Func<T, bool>> predicate)
	{
		WhereString = Parameters<T>.GetWhereString(predicate);
		return this;
	}

	public Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector)
	{
		SelectColumns = Parameters<T>.GetSelectString(selector, StatementList);
		return this;
	}

	private void AddToSelectedColumns(AttributeHelpers.Property property, string modelName)
	{
		var statement = StatementList.Find(x => x.Name == modelName);
		string tableName = statement.TableName;
		string? columnName = statement.Columns.Find(x => x.PropertyName == property.Name)?.ColumnName;
		if (columnName is not null)
		{
			AllColumnsList.Add($"{tableName}.{columnName} AS '{tableName}.{columnName}'");
		}	
	}

	private string FindAllRelations(AttributeHelpers.ClassProps modelProps, Type parentType)
	{
		string joinString = string.Empty;
		Dictionary<AttributeHelpers.ClassProps, (string columnName, string fieldName, bool isList)> relatedModels = new();

		foreach (var property in modelProps.Properties.Where(x => x.Type.Name != parentType?.Name)) 
		{
			AddToSelectedColumns(property, modelProps.ClassName);

			if (property.Attributes.Any(x => x.Name.Contains("OneToOne")))
			{
				var relatedModel = AttributeHelpers.GetPropsByModel(property.Type);
				string columnName = relatedModel.Properties
					.Find(x => x.Attributes.Any(x => x.Name.Contains("OneToOne"))
							&& x.Type.Name == modelProps.ClassName).ColumnName;

				joinString += $"JOIN {relatedModel.TableName} ON {relatedModel.TableName}.{columnName} = {modelProps.TableName}.Id ";
				relatedModels.Add(relatedModel, (columnName, property.Name, false));
			}
			else if (property.Attributes.Any(x => x.Name.Contains("OneToMany")))
			{
				var relatedModel = AttributeHelpers.GetPropsByModel(property.Type.GetGenericArguments()[0]);
				string columnName = relatedModel.Properties
					.Find(x => x.Attributes.Any(x => x.Name.Contains("ManyToOne"))
												&& x.Type.Name == modelProps.ClassName).ColumnName;

				joinString += $"JOIN {relatedModel.TableName} ON {relatedModel.TableName}.{columnName} = {modelProps.TableName}.Id ";
				relatedModels.Add(relatedModel, (columnName, property.Name, true));
			}
		}

		foreach (var relatedModel in relatedModels)
		{
			joinString += FindAllRelations(relatedModel.Key, modelProps.Instance.GetType());
		}

		return joinString;
	}

	private int InsertInto(object model, int id = 0)
	{
		AttributeHelpers.ClassProps modelProps = AttributeHelpers.GetPropsByModel(model.GetType());
		List<string> columns = new();
		List<string> values = new();
		List<object> modelQueue = new();

		foreach (var property in model.GetType().GetProperties())
		{
			bool isRelational = property.GetAttributes().Any(x => x.Name.Contains("OneToOne"));
			var columnName = property.Name;
			var columnValue = property.GetValue(model);

			if ((columnValue is null && !isRelational) 
				|| property.GetAttributes().Any(x => x.Name == "PrimaryGeneratedColumn"))
			{
				continue;
			}

			if (columnValue?.GetType() == typeof(string) && !isRelational)
			{
				columnValue = $"'{columnValue}'";
			}

			if (property.GetAttributes().Any(x => x.Name.Contains("OneToOne")))
			{
				Relationship relationship = (Relationship)modelProps.Properties
					.Find(x => x.Name == property.Name).AttributeProps
					.FirstOrDefault(x => x.Key == "Relationship").Value;

				if (relationship == Relationship.Optional)
				{
					modelQueue.Add(columnValue);
					continue;
				}
				else
				{
					columnName = $"{columnName}Id";
					columnValue = id;
				}
			}

			columns.Add(columnName);
			values.Add(columnValue.ToString());
		}

		var sql = $"INSERT INTO {modelProps.TableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)});";
		sql += " SELECT LAST_INSERT_ID();";

		var result = DbHandler.Query(sql);
		int insertedId = Convert.ToInt32(result.Rows[0][0]);

		foreach (var modelValue in modelQueue)
		{
			int relInsertedId = InsertInto(modelValue, insertedId);
			sql = $"UPDATE {modelProps.TableName} SET {modelValue.GetType().Name}Id = {relInsertedId} WHERE Id = {insertedId}";
			DbHandler.Execute(sql);
		}

		return insertedId;
	}

	private IEnumerable<S> MapData<S>(DataTable table, S instance = null) where S : class, new()
	{
		ModelStatement statement;
		List<S> result = new();
		S obj;
		PropertyInfo[] props;

		if (instance is null)
		{
			statement = StatementList.FirstOrDefault(x => x.Name == typeof(S).Name);
			props = typeof(S).GetProperties();
		} 
		else
		{
			statement = StatementList.FirstOrDefault(x => x.Name == instance.GetType().Name);
			props = instance.GetType().GetProperties();
		}

		foreach (DataRow row in table.Rows)
		{
			obj = instance is null ? new S() : Activator.CreateInstance(instance.GetType()) as S;
			foreach (var prop in props)
			{
				string columnName = statement.Columns.FirstOrDefault(x => x.PropertyName == prop.Name)?.ColumnName;

				if (table.Columns.Contains($"{statement.TableName}.{columnName}") && row[$"{statement.TableName}.{columnName}"] != DBNull.Value)
				{
					if (IsSimpleType(prop.PropertyType))
					{
						prop.SetValue(obj, Convert.ChangeType(row[$"{statement.TableName}.{columnName}"], prop.PropertyType));
					}
					else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
					{
						var nestedInstance = Activator.CreateInstance(prop.PropertyType);
						var nestedObj = MapData(table, nestedInstance); 
						prop.SetValue(obj, nestedObj.FirstOrDefault());
						nestedObj.FirstOrDefault()?.GetType().GetProperty(obj.GetType().Name).SetValue(nestedObj.FirstOrDefault(), obj);
					}
				}
				else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
				{
					Type itemType = prop.PropertyType.GetGenericArguments()[0];
					var nestedInstance = Activator.CreateInstance(itemType);
					var nestedList = MapData(table, nestedInstance); 
					object relListInstance = Activator.CreateInstance(prop.PropertyType);
					IList relList = (IList)relListInstance;

					foreach (var nestedItem in nestedList)
					{
						nestedItem.GetType().GetProperty(obj.GetType().Name).SetValue(nestedItem, obj);
						relList.Add(nestedItem);
					}

					prop.SetValue(obj, relList);
				}
			}

			string pkName = statement.Columns.FirstOrDefault().PropertyName;

			if (result.Any(x => x.GetType().GetProperty(pkName).GetValue(x).Equals(obj.GetType().GetProperty(pkName).GetValue(obj))))
			{
				continue;
			}

			result.Add(obj);
		}
		
		return result;
	}

	private bool IsSimpleType(Type type)
	{
		return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal);
	}

}
