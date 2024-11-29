using System.Linq.Expressions;
using MyORM.Methods;
using MyORM.Querying.Abstract;
using MyORM.Models;
using MyORM.Querying.Enums;
using MyORM.Querying.Functions;
using MyORM.Querying.Models;
using MyORM.DBMS;

namespace MyORM.Querying.Repository;

/// <summary>
/// Repository class for handling database operations.
/// </summary>
/// <typeparam name="T">Type of model.</typeparam>
public class Repository<T> : IRepository<T> where T : class, new()
{
    /// <summary>
    /// Model type.
    /// </summary>
	private Type model;

    /// <summary>
    /// Database handler instance.
    /// </summary>
	private readonly DbHandler _dbHandler;

    /// <summary>
    /// Repository helper instance.
    /// </summary>
	private RepositoryHelper repositoryHelper;

    /// <summary>
    /// List of model statements.
    /// </summary>
	private List<ModelStatement> statementList;

    /// <summary>
    /// Selected columns.
    /// </summary>
	private string selectColumns = string.Empty;

    /// <summary>
    /// Where string.
    /// </summary>
	private string whereString = string.Empty;

    /// <summary>
    /// Order by column.
    /// </summary>
	private string orderByColumn = string.Empty;

    /// <summary>
    /// Gets the model properties.
    /// </summary>
    private AttributeHelpers.ClassProps modelProps
    {
        get
        {
            return AttributeHelpers.GetPropsByModel(model);
        }
    }

    /// <summary>
    /// Gets the model statement.
    /// </summary>
    private ModelStatement statement
    {
        get
        {
            return statementList.GetModelStatement(model.Name);
        }
    }

    /// <summary>
    /// Constructor for the <see cref="Repository{T}"/> class.
    /// </summary>
    /// <param name="dbHandler">Database handler instance</param>
    public Repository(DbHandler dbHandler)
	{
		model = typeof(T);
		_dbHandler = dbHandler;

		ScriptBuilder.Database = dbHandler.AccessLayer.Options.Database;

		repositoryHelper = new RepositoryHelper(dbHandler, model, selectColumns);

		statementList = repositoryHelper.StatementList;
    }

    /// <summary>
    /// Creates a model.
    /// </summary>
    /// <param name="model">Model to create</param>
    /// <returns>Returns the number of rows affected</returns>
    /// <exception cref="Exception">Error while creating model</exception>
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

    /// <summary>
    /// Finds a model.
    /// </summary>
    /// <returns>Returns the model</returns>
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

    /// <summary>
    /// Finds one model.
    /// </summary>
    /// <returns>Returns the model</returns>
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

    /// <summary>
    /// Saves a model.
    /// </summary>
    /// <param name="model">Model to save</param>
    /// <exception cref="Exception">Error updating model</exception>
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

    /// <summary>
    /// Deletes a model.
    /// </summary>
    /// <exception cref="Exception">Error deleting model</exception>
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

    /// <summary>
    /// Deletes a model.
    /// </summary>
    /// <param name="model">Model to delete</param>
    /// <exception cref="Exception">Error deleting model</exception>
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

    /// <summary>
    /// Order by predicate.
    /// </summary>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <param name="selector">Selector to order by</param>
    /// <param name="order">Order by</param>
    /// <returns>Returns the repository</returns>
    public Repository<T> OrderBy<TResult>(Expression<Func<T, TResult>> selector, OrderBy order)
    {
        orderByColumn = Parameters<T>.GetOrderString(selector, order, statementList);
        return this;
    }

    /// <summary>
    /// Where predicate.
    /// </summary>
    /// <param name="predicate">Predicate to where</param>
    /// <returns>Returns the repository</returns>
    public Repository<T> Where(Expression<Func<T, bool>> predicate)
    {
        whereString = Parameters<T>.GetWhereString(predicate);
        return this;
    }

    /// <summary>
    /// Select predicate.
    /// </summary>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <param name="selector">Selector to select</param>
    /// <returns>Returns the repository</returns>
    public Repository<T> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        selectColumns = Parameters<T>.GetSelectString(selector, statementList);
        return this;
    }

    /// <summary>
    /// Clears the predicates.
    /// </summary>
	private void ClearPredicates()
	{
        orderByColumn = string.Empty;
        whereString = string.Empty;
        selectColumns = string.Empty;
    }
}
