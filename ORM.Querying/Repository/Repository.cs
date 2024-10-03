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
	private string SelectColumns { get; set; } = "*";

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
		return FindWithNestedRelations<T>(ModelProps, null);
	}

	public T? FindOne()
	{
		var sql = $"SELECT {SelectColumns} FROM {ModelProps.TableName} {WhereString} LIMIT 1";
		var result = DbHandler.Query(sql);
		return ConvertDataTable<T>(result).FirstOrDefault();
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
		SelectColumns = Parameters<T>.GetSelectString(selector);
		return this;
	}

	private IEnumerable<S> FindWithNestedRelations<S>(AttributeHelpers.ClassProps modelProps, Type parentType, S instance = null) where S : class, new()
	{
		Dictionary<AttributeHelpers.ClassProps, (string columnName, string fieldName, bool isList)> relatedModels = new();

		foreach (var property in modelProps.Properties.Where(x => x.Type.Name != parentType?.Name))
		{
			if (property.Attributes.Any(x => x.Name.Contains("OneToOne")))
			{
				var relatedModel = AttributeHelpers.GetPropsByModel(property.Type);
				string columnName = relatedModel.Properties
					.Find(x => x.Attributes.Any(x => x.Name.Contains("OneToOne"))
							&& x.Type.Name == modelProps.ClassName).ColumnName;

				relatedModels.Add(relatedModel, (columnName, property.Name, false));
			}
			else if (property.Attributes.Any(x => x.Name.Contains("OneToMany")))
			{
				var relatedModel = AttributeHelpers.GetPropsByModel(property.Type.GetGenericArguments()[0]);
				string columnName = relatedModel.Properties
					.Find(x => x.Attributes.Any(x => x.Name.Contains("ManyToOne"))
												&& x.Type.Name == modelProps.ClassName).ColumnName;

				relatedModels.Add(relatedModel, (columnName, property.Name, true));
			}
		}

		var sql = $"SELECT {SelectColumns} FROM {modelProps.TableName} {WhereString} {OrderByColumn}";
		var result = DbHandler.Query(sql);
		var data = instance == null ? ConvertDataTable<S>(result) : ConvertDataTable(result, instance);

		foreach (var relatedModel in relatedModels)
		{
			var typ = relatedModel.Key.Instance;

			foreach (var item in data)
			{
				var relSql = $"SELECT {SelectColumns} FROM {relatedModel.Key.TableName} " +
				$"WHERE {relatedModel.Value.columnName} = {item.GetType().GetProperty("Id").GetValue(item)}";
				var relResult = DbHandler.Query(relSql);
				var relData = ConvertDataTable(relResult, typ);

				if (relatedModel.Value.isList)
				{
					var prop = item.GetType().GetProperty(relatedModel.Value.fieldName);
					object relListInstance = Activator.CreateInstance(prop.PropertyType);
					IList relList = (IList)relListInstance;

					foreach (var relItem in relData)
					{
						relItem.GetType().GetProperty(modelProps.ClassName).SetValue(relItem, item);
						relList.Add(relItem);
					}

					prop.SetValue(item, relList);
				}
				else
				{
					relData.FirstOrDefault().GetType().GetProperty(modelProps.ClassName).SetValue(relData.FirstOrDefault(), item);
					item.GetType().GetProperty(relatedModel.Value.fieldName).SetValue(item, relData.FirstOrDefault());
				}

				FindWithNestedRelations(relatedModel.Key, modelProps.Instance.GetType(), relatedModel.Key.Instance);
			}
		}

		return data;
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

	private IEnumerable<T> ConvertDataTable<T>(DataTable table, T model = null) where T : class, new()
	{
		IList<T> list = new List<T>();
		foreach (DataRow row in table.Rows)
		{
			T obj = model == null ? new T() : model;
			foreach (DataColumn column in table.Columns)
			{
				PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName);
				if (prop != null && row[column] != DBNull.Value)
				{
					prop.SetValue(obj, row[column]);
				}
			}
			list.Add(obj);
		}
		return list;
	}
}
