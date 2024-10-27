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
using MyORM.Querying.Enums;
using MyORM.Querying.Functions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using MySqlX.XDevAPI.Relational;
using MyORM.Querying.Models;

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

	private DataConverter dataConverter;

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
			dataConverter = new DataConverter(StatementList);
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

		AllColumnsList.Clear();
		var sql = $"SELECT {selectColumns} FROM {ModelProps.TableName} {join} {WhereString} {OrderByColumn}";
		var result = DbHandler.Query(sql);
		var data = ConvertData<T>(result);
		return data;
	}

	public T? FindOne()
	{
		string join = FindAllRelations(ModelProps, null);
		string selectColumns = SelectColumns.Length > 0
			? SelectColumns
			: AllColumnsString;

		var sql = $"SELECT {selectColumns} FROM {ModelProps.TableName} {join} {WhereString} {OrderByColumn}";
		var result = DbHandler.Query(sql);
		var data = ConvertData<T>(result);

		return data.FirstOrDefault();
	}

	public void Save(T model)
	{
		List<UpdateData> updateData = new();

		GetUpdateString(model, updateData);

		try
		{
			DbHandler.BeginTransaction();

			foreach (var data in updateData)
			{
				var sql = $"UPDATE {data.TableName} SET {string.Join(", ", data.ColumnValues)} WHERE {data.WhereClause}";
				int affected = DbHandler.Execute(sql);

				if (affected == 0)
				{
					sql = $"INSERT INTO {data.TableName} ({string.Join(", ", data.Columns)}) VALUES ({string.Join(", ", data.Values)})";
					DbHandler.Execute(sql);

					if (data.ManyToManyData is not null)
					{
						sql = " SELECT LAST_INSERT_ID();";
						var result = DbHandler.Query(sql);
						int insertedId = Convert.ToInt32(result.Rows[0][0]);

						sql = $"INSERT INTO {data.ManyToManyData.TableName} " +
							$"({data.ManyToManyData.ColumnName2}, {data.ManyToManyData.ColumnName}) " +
							$"VALUES ({insertedId}, {data.ManyToManyData.ColumnValue})";
						DbHandler.Execute(sql);
					}
				}
				else if (data.ManyToManyData is not null)
				{
					sql = $"SELECT * FROM {data.ManyToManyData.TableName} WHERE " +
						$"{data.ManyToManyData.ColumnName} = {data.ManyToManyData.ColumnValue} " +
						$"AND {data.ManyToManyData.ColumnName2} = {data.ManyToManyData.ColumnValue2}";

					var result = DbHandler.Query(sql);
					bool exists = result.Rows.Count > 0;
					
					if (!exists)
					{
						sql = $"INSERT INTO {data.ManyToManyData.TableName} " +
							$"({data.ManyToManyData.ColumnName2}, {data.ManyToManyData.ColumnName}) " +
							$"VALUES ({data.ManyToManyData.ColumnValue2}, {data.ManyToManyData.ColumnValue})";
						DbHandler.Execute(sql);
					}
				}
			}
		}
		catch (Exception e)
		{
			DbHandler.RollbackTransaction();
			throw new Exception($"Error updating model: {e}", e);
		}
		finally
		{
			DbHandler.CommitTransaction();
		}
	}

	public void UpdateMany(T model)
	{
		var columns = new List<string>();
		var values = new List<string>();

		foreach (var property in model.GetType().GetProperties())
		{
			var columnName = property.Name;
			var columnValue = property.GetValue(model);

			if (columnValue is null || property.HasAttribute("PrimaryGeneratedColumn"))
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
			if (property.HasAttribute("PrimaryGeneratedColumn"))
			{
				WhereString = $"WHERE {property.Name} = {property.GetValue(model)}";
			}
		}

		var sql = $"DELETE FROM {ModelProps.TableName} {WhereString}";
		DbHandler.Execute(sql);
	}

	public Repository<T> OrderBy<TResult>(Expression<Func<T, TResult>> selector, OrderBy order)
	{
		OrderByColumn = Parameters<T>.GetOrderString(selector, order, StatementList);
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

		foreach (var property in modelProps.Properties) 
		{
			AddToSelectedColumns(property, modelProps.ClassName);

			if (property.Type.Name == parentType?.Name) continue;

			if (property.HasAttribute("OneToOne"))
			{
				var relatedModel = AttributeHelpers.GetPropsByModel(property.Type);
				string columnName = relatedModel.Properties.WithNameAndAttribute(modelProps.ClassName, "OneToOne").ColumnName;

				joinString += $"LEFT JOIN {relatedModel.TableName} ON {relatedModel.TableName}.{columnName} = {modelProps.TableName}.Id ";
				relatedModels.Add(relatedModel, (columnName, property.Name, false));
			}
			else if (property.HasAttribute("OneToMany"))
			{
				var relatedModel = AttributeHelpers.GetPropsByModel(property.Type.GetGenericArguments()[0]);
				string columnName = relatedModel.Properties.WithNameAndAttribute(modelProps.ClassName, "ManyToOne").ColumnName;

				joinString += $"LEFT JOIN {relatedModel.TableName} ON {relatedModel.TableName}.{columnName} = {modelProps.TableName}.Id ";
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
			bool isRelational = property.HasAttribute("OneToOne");
			var columnName = property.Name;
			var columnValue = property.GetValue(model);

			if ((columnValue is null && !isRelational) 
				|| property.HasAttribute("PrimaryGeneratedColumn"))
			{
				continue;
			}

			if (columnValue?.GetType() == typeof(string) && !isRelational)
			{
				columnValue = $"'{columnValue}'";
			}

			if (isRelational)
			{
				Relationship relationship = modelProps.Properties.GetRelationship(property.Name);

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

	private void GetUpdateString<T>(T model, List<UpdateData> updateData, Type parentType = null, int? pkValue = null) where T : class, new()
	{
		List<string> columns = new();
		List<string> values = new();
		List<string> whereString = new();

		UpdateData data = new UpdateData();

		ModelStatement statement = StatementList.GetModelStatement(model.GetType().Name);

		if (parentType is not null)
		{
			ModelStatement parentStatement = StatementList.GetModelStatement(parentType.Name);

			if (parentType.HasToManyAttribute("ManyToMany", model.GetType().Name))
			{
				string relationName = model
					.GetType()
					.GetProperties()
					.First(x => x.HasAttribute("ManyToMany")
						&& x.PropertyType.GetGenericArguments()[0].Name == parentType.Name)
					.Name;

				RelationStatement relationStatement = statement.GetRelationStatement(relationName);
				string parentPkName = parentStatement.GetColumn("Id").ColumnName;

				data.ManyToManyData = new ManyToManyData
				{
					TableName = relationStatement.TableName,
					ColumnName = $"{relationStatement.ColumnName_1}",
					ColumnValue = pkValue.ToString(),
					ColumnName2 = $"{relationStatement.ColumnName}",
					ColumnValue2 = model.GetPropertyValue(statement.GetPrimaryKeyPropertyName()).ToString()
				};
			}
			else if (parentType.HasToManyAttribute("OneToMany", model.GetType().Name))
			{
				string fkName = statement.GetColumn(parentType.Name).ColumnName;

				columns.Add($"{statement.TableName}.{fkName}");
				values.Add(pkValue.ToString());
			}
		}

		foreach (var property in model.GetType().GetProperties())
		{
			if (parentType?.Name == property.PropertyType.Name
				|| (property.PropertyType.GetGenericArguments().Count() > 0
					&& parentType?.Name == property.PropertyType.GetGenericArguments()[0].Name))
			{
				continue;
			}

			if (property.HasAttribute("OneToOne"))
			{
				GetUpdateString(property.GetValue(model), updateData, model.GetType());
				continue;
			}

			if (property.HasAttribute("OneToMany") || property.HasAttribute("ManyToMany"))
			{
				if (property.PropertyType.GetGenericArguments().Count() > 0
					&& model.GetType().Name == property.PropertyType.GetGenericArguments()[0].Name)
					continue;

				foreach (var item in (IEnumerable)property.GetValue(model))
					GetUpdateString(item, updateData, model.GetType(), (int)model.GetPropertyValue(statement.GetPrimaryKeyPropertyName()));

				continue;
			}

			if (property.HasAttribute("ManyToMany") || property.HasAttribute("OneToMany") || property.HasAttribute("ManyToOne"))
			{
				continue;
			}

			var columnName = statement.GetColumn(property.Name).ColumnName;
			var columnValue = property.GetValue(model);

			if (property.HasAttribute("PrimaryGeneratedColumn"))
			{
				whereString.Add($"{columnName} = {property.GetValue(model)}");
				continue;
			}

			if (columnValue is null)
				continue;

			if (columnValue.GetType() == typeof(string))
			{
				columnValue = $"'{columnValue}'";
			}
			else if (columnValue.GetType() == typeof(DateTime))
			{
				DateTime dateTimeValue = (DateTime)columnValue;
				columnValue = $"'{dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss")}'";
			}

			columns.Add($"{columnName}");
			values.Add(columnValue.ToString());
		}

		data.TableName = statement.TableName;
		data.Columns = columns;
		data.Values = values;
		data.WhereClause = string.Join(" AND ", whereString);

		updateData.Add(data);
	}

	private IEnumerable<S> ConvertData<S>(DataTable table) where S : class, new()
	{
		var data = dataConverter.MapData<S>(table);
		data = MapManyToMany(data.ToList());

		return data;
	}

	private IEnumerable<S> MapManyToMany<S>(IEnumerable<S> data, S instance = null, Type parentType = null) where S: class, new()
	{
		ModelStatement statement;
		PropertyInfo[] props; 
		
		if (instance is null)
		{
			statement = StatementList.Find(x => x.Name == typeof(S).Name);
			props = typeof(S).GetProperties();
		}
		else
		{
			statement = StatementList.Find(x => x.Name == instance.GetType().Name);
			props = instance.GetType().GetProperties();
		}

		foreach (var obj in data)
		{
			foreach (var prop in props.WithAttributes(["ManyToMany"]))
			{
				var relStatement = statement.Relationships.Find(x => x.PropertyName == prop.Name);
				string joinString = FindAllRelations(ModelProps, null);
				var modelProps = ModelProps;
				Type typeOfList = prop.PropertyType.GetGenericArguments()[0];
				var nestedInstance = Activator.CreateInstance(typeOfList);

				if (instance is not null)
				{
					AllColumnsList.Clear();
					joinString = string.Empty;
					modelProps = AttributeHelpers.GetPropsByModel(prop.PropertyType.GetGenericArguments()[0]);

					foreach (var property in modelProps.Properties)
					{
						AddToSelectedColumns(property, modelProps.ClassName);
					}
				}
				string selectColumns = SelectColumns.Length > 0
					? SelectColumns
					: AllColumnsString;

				AllColumnsList.Clear();
				var sql =
					$"SELECT {selectColumns} FROM {modelProps.TableName} {joinString} " +
					$"LEFT JOIN {relStatement.TableName} ON {relStatement.TableName}.{relStatement.ColumnName} = {obj.GetPropertyValue("Id")} " +
					$"WHERE {modelProps.TableName}.Id = {relStatement.TableName}.{relStatement.ColumnName_1}";
				var sqlResult = DbHandler.Query(sql);
				var result = dataConverter.MapData(sqlResult, nestedInstance);

				IList relList = (IList)Activator.CreateInstance(prop.PropertyType);
				foreach (var item in result)
				{
					relStatement = statement.Relationships.Find(x => x.PropertyName == prop.Name);
					relList.Add(item);
				}

				prop.SetValue(obj, relList);
			}

			foreach (var prop in props.WithAttributes(["OneToOne"]).Where(x => x.PropertyType.Name != parentType?.Name))
			{
				object relListInstance = Activator.CreateInstance(prop.PropertyType);
				var objValue = obj.GetPropertyValue(prop.Name);
				IEnumerable<object> relList = new List<object>()
				{
					objValue
				};

				var result = MapManyToMany(relList, relListInstance, obj.GetType());
				prop.SetValue(obj, result.FirstOrDefault());
			}

			foreach (var prop in props.WithAttributes(["OneToMany"]))
			{
				List<object> relObj = new List<object>();
				var objValue = obj.GetPropertyValue(prop.Name);
				Type typeOfList = prop.PropertyType.GetGenericArguments()[0];
				var nestedInstance = Activator.CreateInstance(typeOfList);

				foreach (var item in objValue as IEnumerable)
				{
					relObj.Add(item);
				}

				var result = MapManyToMany(relObj, nestedInstance);

				IList relList = (IList)Activator.CreateInstance(prop.PropertyType);
				foreach (var item in result)
				{
					relList.Add(item);
				}

				prop.SetValue(obj, relList);
			}
		}

		return data;
	}
}
