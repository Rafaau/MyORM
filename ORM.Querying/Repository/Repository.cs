using ORM.Abstract;
using ORM.Attributes;
using ORM.Common;
using ORM.Common.Methods;
using System.Data;
using System.Reflection;
using ORM.Querying.Abstract;
using Org.BouncyCastle.Asn1.X509;

namespace ORM.Querying;

public class Repository<T> : IRepository<T> where T : new()
{
    private Type Model { get; set; }
    private Schema Schema { get; set; }
    private AttributeHelpers.ClassProps ModelProps { get; set; }

    public Repository(AccessLayer accessLayer)
    {
        Model = typeof(T);

        var connectionString = accessLayer.ConnectionString;
        Schema = new Schema(connectionString);
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
            if (columnValue.GetType() == typeof(string))
            {
                columnValue = $"'{columnValue}'";
            }

            if (columnValue is null || property.GetAttributes().Any(x => x.Name == "PrimaryGeneratedColumn"))
            {
                continue;
            }

            columns.Add(columnName);
            values.Add(columnValue.ToString());
        }

        var sql = $"INSERT INTO {ModelProps.TableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";

        Schema.Execute(sql);
    }

    public IEnumerable<T> Find()
    {
        var sql = $"SELECT * FROM {ModelProps.TableName}";
        var result = Schema.Query(sql);
        return ConvertDataTable<T>(result);
    }

    public IEnumerable<T> Find(Where? where)
    {
        string? whereString = where.GetWhereString();

        var sql = $"SELECT * FROM {ModelProps.TableName} {whereString}";
        var result = Schema.Query(sql);
        return ConvertDataTable<T>(result);
    }

    public IEnumerable<T> Find(Order? order)
    {
        string? orderString = order.GetOrderString();

        var sql = $"SELECT * FROM {ModelProps.TableName} {orderString}";
        var result = Schema.Query(sql);
        return ConvertDataTable<T>(result);
    }

    public IEnumerable<T> Find(Where? where, Order? order)
    {
        string? whereString = where.GetWhereString();
        string? orderString = order.GetOrderString();

        var sql = $"SELECT * FROM {ModelProps.TableName} {whereString} {orderString}";
        var result = Schema.Query(sql);
        return ConvertDataTable<T>(result);
    }

    public T? FindOne(Where where)
    {
        string whereString = where.GetWhereString();

        var sql = $"SELECT * FROM {ModelProps.TableName} {whereString} LIMIT 1";
        var result = Schema.Query(sql);
        return ConvertDataTable<T>(result).FirstOrDefault();
    }

    public void Update(Where where, T model)
    {
        string whereString = where.GetWhereString();
        string updateString = StringHelpers.GetUpdateString(model);

        var sql = $"UPDATE {ModelProps.TableName} SET {updateString} {whereString}";
        Schema.Execute(sql);
    }

    public void Delete(Where where)
    {
        string whereString = where.GetWhereString();

        var sql = $"DELETE FROM {ModelProps.TableName} {whereString}";
        Schema.Execute(sql);
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
