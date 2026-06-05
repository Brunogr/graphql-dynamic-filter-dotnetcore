#nullable disable
using DynamicQuery.Parser.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DynamicQuery.Parser
{
    public class ExpressionParser
    {
        public ExpressionParser(string filterValues, Type itemType)
        {
            Properties = new List<PropertyInfo>();

            var values = DefineOperation(filterValues, itemType);

            Value = ParseValue(values[1]);
        }

        public ExpressionParser()
        {
            Properties = new List<PropertyInfo>();
        }

        public List<PropertyInfo> Properties { get; set; }
        public object Value { get; set; }
        public OperatorEnum Condition { get; set; }

        private object ParseValue(string value)
        {
            object parsedValue = null;

            foreach (var property in Properties)
            {
                if (property.PropertyType.IsClass && property.PropertyType.Name.ToLower() != "string" && property.PropertyType.Name.ToLower() != "datetime")
                    continue;

                if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                    var type = typeof(Nullable<>).MakeGenericType(underlyingType);

                    object newValue;

                    if (underlyingType.IsEnum)
                    {
                        newValue = Enum.Parse(underlyingType, value);
                    }
                    else
                    {
                        newValue = ChangeType(value, underlyingType);
                    }

                    var nullableObject = Activator.CreateInstance(type, newValue);

                    parsedValue = nullableObject;
                }
                else
                {
                    parsedValue = ChangeType(value, property.PropertyType);
                }
            }

            return parsedValue;
        }

        private object ChangeType(string value, Type type)
        {
            if (type.IsEnum)
                return Convert.ChangeType(Enum.Parse(type, value), type);

            if (type == typeof(Guid))
                return Guid.Parse(value);

            var converter = TypeDescriptor.GetConverter(type);

            return converter.ConvertFrom(value);
        }

        public string GetOperation()
        {
            switch (Condition)
            {
                case OperatorEnum.Equals:
                    return "=";
                case OperatorEnum.Contains:
                    return "%";
                case OperatorEnum.ContainsCaseSensitive:
                    return "%%";
                case OperatorEnum.GreaterThan:
                    return ">";
                case OperatorEnum.LessThan:
                    return "<";
                case OperatorEnum.GreaterOrEqual:
                    return ">=";
                case OperatorEnum.LessOrEqual:
                    return "<=";
                case OperatorEnum.NotEquals:
                    return "!=";
                default:
                    return "=";
            }
        }

        public string[] DefineOperation(string filterValues, Type itemType)
        {
            string[] values = null;

            if (filterValues.Contains('='))
            {
                values = filterValues.Split('=');
                Condition = OperatorEnum.Equals;
            }

            if (filterValues.Contains('%'))
            {
                if (filterValues.Contains("%%"))
                {
                    Condition = OperatorEnum.ContainsCaseSensitive;
                    values = Regex.Split(filterValues, "%%");
                }
                else
                {
                    Condition = OperatorEnum.Contains;
                    values = filterValues.Split('%');
                }
            }

            if (filterValues.Contains('>'))
            {
                values = filterValues.Split('>');
                Condition = OperatorEnum.GreaterThan;
            }

            if (filterValues.Contains('<'))
            {
                values = filterValues.Split('<');
                Condition = OperatorEnum.LessThan;
            }

            if (filterValues.Contains(">="))
            {
                values = Regex.Split(filterValues, ">=");
                Condition = OperatorEnum.GreaterOrEqual;
            }

            if (filterValues.Contains("<="))
            {
                values = Regex.Split(filterValues, "<=");
                Condition = OperatorEnum.LessOrEqual;
            }

            if (filterValues.Contains("!="))
            {
                values = Regex.Split(filterValues, "!=");
                Condition = OperatorEnum.NotEquals;
            }

            if (values == null)
                throw new ArgumentNullException(nameof(filterValues));

            var property = itemType.GetProperty(values[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

            if (property != null)
                Properties.Add(property);
            else
                Properties = GetNestedProp(values[0], itemType);

            if (Properties == null || !Properties.Any())
                throw new PropertyNotFoundException(values[0], itemType.Name);

            return values;
        }

        public List<PropertyInfo> GetNestedProp(string name, Type obj)
        {
            var infos = new List<PropertyInfo>();
            foreach (var part in name.Split('.'))
            {
                if (obj == null)
                    return null;

                if (obj.IsGenericType && obj.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
                {
                    obj = obj.GetGenericArguments().FirstOrDefault();
                }

                var info = obj.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);
                if (info == null)
                    return infos;

                obj = info.PropertyType;
                infos.Add(info);
            }

            return infos;
        }

        public Expression GetExpression(ParameterExpression parameter)
        {
            var constantExpression = Expression.Constant(Value);
            Expression returnExpression;
            ParameterExpression subParam = null;
            Expression baseExp = null;
            Type genericType = null;

            if (Nullable.GetUnderlyingType(Properties.LastOrDefault().PropertyType) != null)
            {
                var type = typeof(Nullable<>).MakeGenericType(Nullable.GetUnderlyingType(Properties.LastOrDefault().PropertyType));
                constantExpression = Expression.Constant(Value, type);
            }

            Expression body = parameter;
            foreach (var member in Properties)
            {
                if (member.PropertyType.IsGenericType && member.PropertyType.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsAssignableFrom(typeof(IEnumerable<>))))
                {
                    genericType = member.PropertyType;
                    baseExp = Expression.Property(body, member);
                    body = Expression.Property(body, member);
                    continue;
                }

                if (genericType != null)
                {
                    subParam = Expression.Parameter(genericType.GetGenericArguments().FirstOrDefault(), "y");
                    body = Expression.Property(subParam, member);
                }
                else
                {
                    body = Expression.Property(body, member);
                }
            }

            switch (Condition)
            {
                default:
                case OperatorEnum.Equals:
                    returnExpression = Expression.Equal(body, constantExpression);
                    break;
                case OperatorEnum.Contains:
                    constantExpression = Expression.Constant(Value.ToString().ToLower());

                    var toLowerMethod = typeof(string).GetMethod("ToLowerInvariant", Type.EmptyTypes);
                    var expression1 = Expression.Call(body, toLowerMethod);
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                    returnExpression = Expression.Call(expression1, containsMethod, constantExpression);
                    break;
                case OperatorEnum.ContainsCaseSensitive:
                    var caseSensitiveContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    returnExpression = Expression.Call(body, caseSensitiveContains, constantExpression);
                    break;
                case OperatorEnum.GreaterThan:
                    returnExpression = Expression.GreaterThan(body, constantExpression);
                    break;
                case OperatorEnum.LessThan:
                    returnExpression = Expression.LessThan(body, constantExpression);
                    break;
                case OperatorEnum.GreaterOrEqual:
                    returnExpression = Expression.GreaterThanOrEqual(body, constantExpression);
                    break;
                case OperatorEnum.LessOrEqual:
                    returnExpression = Expression.LessThanOrEqual(body, constantExpression);
                    break;
                case OperatorEnum.NotEquals:
                    returnExpression = Expression.NotEqual(body, constantExpression);
                    break;
            }

            if (genericType != null)
            {
                var anyMethod = typeof(Enumerable).GetMethods()
                    .Where(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .FirstOrDefault()
                    .MakeGenericMethod(genericType.GetGenericArguments().FirstOrDefault());

                returnExpression = Expression.Call(anyMethod, baseExp, Expression.Lambda(returnExpression, subParam));
            }

            return returnExpression;
        }
    }

    public enum OperatorEnum
    {
        Equals = 1,
        Contains = 2,
        ContainsCaseSensitive = 3,
        GreaterThan = 4,
        LessThan = 5,
        GreaterOrEqual = 6,
        LessOrEqual = 7,
        NotEquals = 8
    }
}
