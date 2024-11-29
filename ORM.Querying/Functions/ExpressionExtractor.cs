using MyORM.DBMS;
using MyORM.Methods;
using MyORM.Models;
using MyORM.Querying.Enums;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MyORM.Querying.Functions;

/// <summary>
/// Class for extracting expressions for the query.
/// </summary>
internal static class ExpressionExtractor
{
    /// <summary>
    /// Processes the expression.
    /// </summary>
    /// <param name="expression">Expression to process</param>
    /// <param name="whereClause">Where clause</param>
    /// <exception cref="NotSupportedException">Exception for unsupported expression type</exception>
    public static void ProcessExpression(Expression expression, StringBuilder whereClause)
	{
		switch (expression.NodeType)
		{
			case ExpressionType.AndAlso:
				BinaryExpression andExpression = (BinaryExpression)expression;
				ProcessExpression(andExpression.Left, whereClause);
				whereClause.Append(" AND ");
				ProcessExpression(andExpression.Right, whereClause);
				break;

			case ExpressionType.Equal:
				BinaryExpression equalExpression = (BinaryExpression)expression;
				var left = equalExpression.Left as MemberExpression;
				var right = ExtractValue(equalExpression.Right);
				var propertyName = left.Member.Name;
				var propertyValueFormatted = FormatValueForSql(right);
                var props = AttributeHelpers.GetPropsByModel(left.Member.DeclaringType);
                whereClause.Append($"{props.TableName}.{props.GetColumnNameByProperty(propertyName)} = {propertyValueFormatted}");
				break;

			case ExpressionType.OrElse:
				BinaryExpression orExpression = (BinaryExpression)expression;
				ProcessExpression(orExpression.Left, whereClause);
				whereClause.Append(" OR ");
				ProcessExpression(orExpression.Right, whereClause);
				break;

			case ExpressionType.NotEqual:
				BinaryExpression notEqualExpression = (BinaryExpression)expression;
				var leftNotEqual = notEqualExpression.Left as MemberExpression;
				var rightNotEqual = ExtractValue(notEqualExpression.Right);
				var propertyNameNotEqual = leftNotEqual.Member.Name;
				var propertyValueFormattedNotEqual = FormatValueForSql(rightNotEqual);
                var propsNotEqual = AttributeHelpers.GetPropsByModel(leftNotEqual.Member.DeclaringType);
                whereClause.Append($"{propsNotEqual.TableName}.{propsNotEqual.GetColumnNameByProperty(propertyNameNotEqual)} " +
					$"!= {propertyValueFormattedNotEqual}");
                break;

			case ExpressionType.LessThan:
				BinaryExpression lessThanExpression = (BinaryExpression)expression;
				var leftLessThan = lessThanExpression.Left as MemberExpression;
				var rightLessThan = ExtractValue(lessThanExpression.Right);
				var propertyNameLessThan = leftLessThan.Member.Name;
				var propertyValueFormattedLessThan = FormatValueForSql(rightLessThan);
                var propsLessThan = AttributeHelpers.GetPropsByModel(leftLessThan.Member.DeclaringType);
                whereClause.Append($"{propsLessThan.TableName}.{propsLessThan.GetColumnNameByProperty(propertyNameLessThan)} " +
                    $"< {propertyValueFormattedLessThan}");
                break;

			case ExpressionType.GreaterThan:
				BinaryExpression greaterThanExpression = (BinaryExpression)expression;
				var leftGreaterThan = greaterThanExpression.Left as MemberExpression;
				var rightGreaterThan = ExtractValue(greaterThanExpression.Right);
				var propertyNameGreaterThan = leftGreaterThan.Member.Name;
				var propertyValueFormattedGreaterThan = FormatValueForSql(rightGreaterThan);
                var propsGreaterThan = AttributeHelpers.GetPropsByModel(leftGreaterThan.Member.DeclaringType);
                whereClause.Append($"{propsGreaterThan.TableName}.{propsGreaterThan.GetColumnNameByProperty(propertyNameGreaterThan)} " +
                    $"< {propertyValueFormattedGreaterThan}");
                break;

			case ExpressionType.LessThanOrEqual:
				BinaryExpression lessThanOrEqualExpression = (BinaryExpression)expression;
				var leftLessThanOrEqual = lessThanOrEqualExpression.Left as MemberExpression;
				var rightLessThanOrEqual = ExtractValue(lessThanOrEqualExpression.Right);
				var propertyNameLessThanOrEqual = leftLessThanOrEqual.Member.Name;
				var propertyValueFormattedLessThanOrEqual = FormatValueForSql(rightLessThanOrEqual);
                var propsLessThanOrEqual = AttributeHelpers.GetPropsByModel(leftLessThanOrEqual.Member.DeclaringType);
                whereClause.Append($"{propsLessThanOrEqual.TableName}.{propsLessThanOrEqual.GetColumnNameByProperty(propertyNameLessThanOrEqual)} " +
                    $"< {propertyValueFormattedLessThanOrEqual}");
                break;

			case ExpressionType.GreaterThanOrEqual:
				BinaryExpression greaterThanOrEqualExpression = (BinaryExpression)expression;
				var leftGreaterThanOrEqual = greaterThanOrEqualExpression.Left as MemberExpression;
				var rightGreaterThanOrEqual = ExtractValue(greaterThanOrEqualExpression.Right);
				var propertyNameGreaterThanOrEqual = leftGreaterThanOrEqual.Member.Name;
				var propertyValueFormattedGreaterThanOrEqual = FormatValueForSql(rightGreaterThanOrEqual);
                var propsGreaterThanOrEqual = AttributeHelpers.GetPropsByModel(leftGreaterThanOrEqual.Member.DeclaringType);
                whereClause.Append($"{propsGreaterThanOrEqual.TableName}.{propsGreaterThanOrEqual.GetColumnNameByProperty(propertyNameGreaterThanOrEqual)} " +
                    $"< {propertyValueFormattedGreaterThanOrEqual}");
                break;

			default:
				throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
		}
	}

    /// <summary>
    /// Extracts the property names.
    /// </summary>
    /// <typeparam name="T">Type of the model</typeparam>
    /// <typeparam name="TResult">Type of the result</typeparam>
    /// <param name="expression">Expression to extract the property names</param>
    /// <param name="parameterType">Type of the parameter</param>
    /// <param name="statementsList">List of model statements</param>
    /// <returns>Returns the list of property names</returns>
    public static List<string> ExtractPropertyNames<T, TResult>(Expression<Func<T, TResult>> expression, ParameterType parameterType, List<ModelStatement> statementsList)
	{
		var names = new List<string>();

		if (expression.Body is NewExpression newExpression)
		{
			foreach (var argument in newExpression.Arguments)
			{
				if (argument is MemberExpression memberExpression)
				{
					ModelStatement statement = statementsList.Find(s => s.Name == memberExpression.Member.DeclaringType.Name);
					string column = parameterType == ParameterType.OrderBy
						? $"{statement.TableName}.{statement.GetColumnName(memberExpression.Member.Name)} "
						: ScriptBuilder.BuildSelect(statement.TableName, statement.GetColumnName(memberExpression.Member.Name));
					names.Add(column);
				}
			}
		}

		return names;
	}

    /// <summary>
    /// Extracts the value from the expression.
    /// </summary>
    /// <param name="expression">Expression to extract the value</param>
    /// <returns>Returns the extracted value</returns>
    /// <exception cref="NotSupportedException">Exception for unsupported member type</exception>
    private static object ExtractValue(Expression expression)
	{
		if (expression is ConstantExpression constant)
		{
			return constant.Value;
		}

		if (expression is MemberExpression memberExpression && memberExpression.Expression is ConstantExpression memberConstant)
		{
			object container = memberConstant.Value;
			switch (memberExpression.Member)
			{
				case FieldInfo field:
					return field.GetValue(container);
				case PropertyInfo property:
					return property.GetValue(container);
				default:
					throw new NotSupportedException($"Unsupported member type: {memberExpression.Member.GetType().Name}");
			}
		}

		var objectMember = Expression.Convert(expression, typeof(object));
		var getterLambda = Expression.Lambda<Func<object>>(objectMember);
		var getter = getterLambda.Compile();
		return getter();
	}

    /// <summary>
    /// Formats the value for SQL.
    /// </summary>
    /// <param name="value">Value to format</param>
    /// <returns>Returns the formatted value</returns>
    private static string FormatValueForSql(object value)
	{
		if (value is string strValue)
			return $"'{strValue}'";
		if (value is DateTime dateTimeValue)
			return $"'{dateTimeValue:yyyy-MM-dd HH:mm:ss}'";
		return value.ToString();
	}
}
