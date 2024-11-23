using System.Linq.Expressions;
using MyORM.Methods;
using MyORM.Querying.Abstract;
using MyORM.Models;
using MyORM.Querying.Enums;
using MyORM.Querying.Functions;
using MyORM.Querying.Models;
using MyORM.DBMS;

namespace MyORM.Querying.Repository;

public class Repository<T> : IRepository<T> where T : class, new()
{
	private Type model;
	private readonly DbHandler _dbHandler;
	private RepositoryHelper repositoryHelper;
	private List<ModelStatement> statementList;
	private string selectColumns = string.Empty;
	private string whereString = string.Empty;
	private string orderByColumn = string.Empty;
    private AttributeHelpers.ClassProps modelProps
    {
        get
        {
            return AttributeHelpers.GetPropsByModel(model);
        }
    }
    private ModelStatement statement
    {
        get
        {
            return statementList.GetModelStatement(model.Name);
        }
    }

    public Repository(DbHandler dbHandler)
	{
		model = typeof(T);
		_dbHandler = dbHandler;

		ScriptBuilder.Database = dbHandler.AccessLayer.Options.Database;

		repositoryHelper = new RepositoryHelper(dbHandler, model, selectColumns);

		statementList = repositoryHelper.StatementList;
    }

	public int Create(T model)
	{
		try
		{
            _dbHandler.BeginTransaction();
			int result = repositoryHelper.InsertInto(model);
            _dbHandler.CommitTransaction();

			return result;
		}
		catch (Exception e)
        {
            _dbHandler.RollbackTransaction();
            throw new Exception("Error while creating model", e);
        }
	}

	public IEnumerable<T> Find()
	{
		string join = repositoryHelper.FindAllRelations(modelProps, null);
		string select = selectColumns.Length > 0 
			? selectColumns
			: repositoryHelper.AllColumnsString;

		repositoryHelper.AllColumnsList.Clear();
		var sql = 
			$"SELECT {select} FROM {modelProps.TableName} " +
			$"{join} " +
			$"{whereString} " +
			$"{orderByColumn}";
		var result = _dbHandler.Query(sql);
		var data = repositoryHelper.ConvertData<T>(result);

        ClearPredicates();

        return data;
	}

	public T? FindOne()
	{
		string join = repositoryHelper.FindAllRelations(modelProps, null);
		string select = selectColumns.Length > 0
			? selectColumns
			: repositoryHelper.AllColumnsString;

		var sql = 
			$"SELECT {select} FROM {modelProps.TableName} " +
			$"{join} " +
			$"{whereString} " +
			$"{orderByColumn}";
		var result = _dbHandler.Query(sql);

		var data = repositoryHelper.ConvertData<T>(result);

        ClearPredicates();

        return data.FirstOrDefault();
	}

	public void Save(T model)
	{
		List<UpdateData> updateData = new();

		repositoryHelper.GetUpdateString(model, updateData);

		try
		{
            _dbHandler.BeginTransaction();
			repositoryHelper.Update(updateData);
            _dbHandler.CommitTransaction();
        }
		catch (Exception e)
		{
            _dbHandler.RollbackTransaction();
			throw new Exception($"Error updating model: {e}", e);
		}
		finally
		{
            ClearPredicates();
        }
	}

    public void Delete()
    {
		try
		{
            _dbHandler.BeginTransaction();
            var sql = $"DELETE FROM {modelProps.TableName} {whereString}";
            _dbHandler.Execute(sql);
            _dbHandler.CommitTransaction();
        }
        catch (Exception e)
		{
            _dbHandler.RollbackTransaction();
            throw new Exception($"Error deleting model: {e}", e);
        }
        finally
		{
            ClearPredicates();
        }
    }

    public void Delete(T model)
    {
		try
		{
            int id = 0;

            foreach (var property in model.GetType().GetProperties())
            {
                if (property.HasAttribute("PrimaryGeneratedColumn"))
                {
                    id = (int)property.GetValue(model);
                }
            }

            var sql =
                $"DELETE FROM {modelProps.TableName} " +
                $"WHERE {statement.GetPrimaryKeyColumnName()} = {id}";
            _dbHandler.Execute(sql);
        }
        catch (Exception e)
		{
            throw new Exception($"Error deleting model: {e}", e);
        }
        finally
		{
            ClearPredicates();
        }
    }

    public Repository<T> OrderBy<TResult>(Expression<Func<T, TResult>> selector, OrderBy order)
    {
        orderByColumn = Parameters<T>.GetOrderString(selector, order, statementList);
        return this;
    }

    public Repository<T> Where(Expression<Func<T, bool>> predicate)
    {
        whereString = Parameters<T>.GetWhereString(predicate);
        return this;
    }

    public Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        selectColumns = Parameters<T>.GetSelectString(selector, statementList);
        return this;
    }

	private void ClearPredicates()
	{
        orderByColumn = string.Empty;
        whereString = string.Empty;
        selectColumns = string.Empty;
    }
}
