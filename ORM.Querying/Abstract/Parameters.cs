using MyORM.Models;
using MyORM.Querying.Functions;
using System.Collections;
using System.Linq.Expressions;
using System.Text;

namespace MyORM.Querying.Abstract;

public static class Parameters<T> where T : new()
{
	public static string GetWhereString(Expression<Func<T, bool>> expression)
	{
		StringBuilder whereClause = new StringBuilder("WHERE ");
		ExpressionExtractor.ProcessExpression(expression.Body, whereClause);
		return whereClause.ToString();
	}

	public static string GetOrderString(string columnName, string order)
	{
		return $"ORDER BY {columnName} {order}";
	}

	public static string GetSelectString<TResult>(Expression<Func<T, TResult>> selector, List<ModelStatement> statementsList)
	{
		var names = ExpressionExtractor.ExtractPropertyNames(selector, statementsList);
		return string.Join(", ", names);
	}
}

public class ExtendedDictionary : IEnumerable<KeyValuePair<string, List<object>>>
{
	private readonly Dictionary<string, List<object>> _internalDict = new();

	public void Add(string key, params object[] values)
	{
		_internalDict[key] = new List<object>(values);
	}

	public IEnumerable<object> this[string key] => _internalDict[key];

	public IEnumerator<KeyValuePair<string, List<object>>> GetEnumerator()
	{
		return _internalDict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _internalDict.GetEnumerator();
	}
}