using ORM.Abstract;
using ORM.Common;
using ORM.Common.Methods;
using System.Data;
using System.Reflection;
using ORM.Querying.Abstract;
using System.Linq.Expressions;

namespace ORM.Querying;

public class Repository<T> : IRepository<T> where T : new()
{
    private Type Model { get; set; }
    private DbHandler DbHandler { get; set; }
    private AttributeHelpers.ClassProps ModelProps { get; set; }

    private string OrderByColumn { get; set; } = string.Empty;
    private string WhereString { get; set; } = string.Empty;
    private string SelectColumns { get; set; } = "*";

    public Repository(DbHandler dbHandler)
    {
        Model = typeof(T);

        DbHandler = dbHandler;
        ModelProps = AttributeHelpers.GetPropsByModel(Model);
    }

    public void Create(T model)
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

            columns.Add(columnName);
            values.Add(columnValue.ToString());
        }

        var sql = $"INSERT INTO {ModelProps.TableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";

        DbHandler.Execute(sql);
    }

    public IEnumerable<T> Find()
    {
        var sql = $"SELECT {SelectColumns} FROM {ModelProps.TableName} {WhereString} {OrderByColumn}";
        var result = DbHandler.Query(sql);
        return ConvertDataTable<T>(result);
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
		this.OrderByColumn = Parameters<T>.GetOrderString(columnName, order);
		return this;
	}

	public Repository<T> Where(Expression<Func<T, bool>> predicate)
	{
		this.WhereString = Parameters<T>.GetWhereString(predicate);
		return this;
	}

    public Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
		this.SelectColumns = Parameters<T>.GetSelectString(selector);
        return this;
	}

	private IEnumerable<T> ConvertDataTable<T>(DataTable table) where T : new()
    {
        IList<T> list = new List<T>();
        foreach (DataRow row in table.Rows)
        {
            T obj = new T();
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
