using MyORM.DBMS;
using MyORM.Methods;
using MyORM.Models;
using MyORM.Querying.Enums;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MyORM.Querying.Functions;

internal static class ExpressionExtractor
{
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

	private static string FormatValueForSql(object value)
	{
		if (value is string strValue)
			return $"'{strValue}'";
		if (value is DateTime dateTimeValue)
			return $"'{dateTimeValue:yyyy-MM-dd HH:mm:ss}'";
		return value.ToString();
	}
}
