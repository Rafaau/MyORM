using MyORM.Models;
using MyORM.Querying.Enums;
using MyORM.Querying.Functions;
using System.Linq.Expressions;
using System.Text;

namespace MyORM.Querying.Abstract;

/// <summary>
/// Class that provides predicates for the query.
/// </summary>
/// <typeparam name="T">Type of the model</typeparam>
public static class Parameters<T> where T : new()
{
    /// <summary>
    /// Gets the where string from the expression.
    /// </summary>
    /// <param name="expression">Expression to get the where string</param>
    /// <returns>Returns the where string</returns>
    public static string GetWhereString(Expression<Func<T, bool>> expression)
	{
		StringBuilder whereClause = new StringBuilder("WHERE ");
		ExpressionExtractor.ProcessExpression(expression.Body, whereClause);
		return whereClause.ToString();
	}

    /// <summary>
    /// Gets the order string from the selector.
    /// </summary>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <param name="selector">Selector to get the order string</param>
    /// <param name="order">Order by</param>
    /// <param name="statementsList">List of model statements</param>
    /// <returns>Returns the order string</returns>
    public static string GetOrderString<TResult>(Expression<Func<T, TResult>> selector, OrderBy order, List<ModelStatement> statementsList)
	{
		var names = ExpressionExtractor.ExtractPropertyNames(selector, ParameterType.OrderBy, statementsList);
		return $"ORDER BY {string.Join(", ", names)} {order}";
	}

    /// <summary>
    /// Gets the select string from the selector.
    /// </summary>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <param name="selector">Selector to get the select string</param>
    /// <param name="statementsList">List of model statements</param>
    /// <returns>Returns the select string</returns>
    public static string GetSelectString<TResult>(Expression<Func<T, TResult>> selector, List<ModelStatement> statementsList)
	{
		var names = ExpressionExtractor.ExtractPropertyNames(selector, ParameterType.Select, statementsList);
		return string.Join(", ", names);
	}
}