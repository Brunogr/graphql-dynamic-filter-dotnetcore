using Graphql.DynamicFilter.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Graphql.DynamicFiltering
{
    public class ExpressionParser
    {
        public ExpressionParser(string filterValues, Type itemType)
        {
            Properties = new List<PropertyInfo>();

            var values = DefineOperation(filterValues, itemType);

            Value = ParseValue(values[1]);
        }

        #region [ Properties ]
        public List<PropertyInfo> Properties { get; set; }
        public object Value { get; set; }
        public OperatorEnum Condition { get; set; }

        #endregion

        #region [ Private Methods ]
        private object ParseValue(string value)
        {
            object parsedValue = null;

            foreach (var property in Properties)
            {
                if (property.PropertyType.IsClass && property.PropertyType.Name.ToLower() != "string" && property.PropertyType.Name.ToLower() != "datetime")
                    continue;
                //Verifying if is nullable
                if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                    var type = typeof(Nullable<>).MakeGenericType(underlyingType);

                    if (underlyingType.IsEnum)
                    {
                        var teste = Enum.Parse(underlyingType, value);
                    }

                    var newValue = ChangeType(value, underlyingType);

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
            
            return Convert.ChangeType(value, type);
        }

        private string[] DefineOperation(string filterValues, Type itemType)
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
                throw new ArgumentNullException("filter");

            var property = itemType.GetProperty(values[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

            if (property != null)
                Properties.Add(property);
            else
                Properties = GetNestedProp(values[0], itemType);

            if (Properties == null || !Properties.Any())
                throw new PropertyNotFoundException(values[0], itemType.Name);

            return values;
        }

        public List<PropertyInfo> GetNestedProp(String name, Type obj)
        {
            List<PropertyInfo> infos = new List<PropertyInfo>();
            foreach (String part in name.Split('.'))
            {
                if (obj == null) { return null; }

                //Type type = obj.GetType();
                var info = obj.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);
                if (info == null) { return infos; }

                obj = info.PropertyType;
                infos.Add(info);
            }
            return infos;
        }

        #endregion

        #region [ Public methods ]
        public Expression GetExpression(ParameterExpression parameter)
        {
            var constantExpression = Expression.Constant(Value);

            //making constant nullable
            if (Nullable.GetUnderlyingType(Properties.LastOrDefault().PropertyType) != null)
            {
                var type = typeof(Nullable<>).MakeGenericType(Nullable.GetUnderlyingType(Properties.LastOrDefault().PropertyType));
                constantExpression = Expression.Constant(Value, type);
            }

            Expression body = parameter;
            foreach (var member in Properties)
            {
                body = Expression.Property(body, member);
            }

            switch (Condition)
            {
                default:
                case OperatorEnum.Equals:
                    {
                        return Expression.Equal(body, constantExpression);
                    }
                case OperatorEnum.Contains:
                    {
                        constantExpression = Expression.Constant(Value.ToString().ToLower());
                                                
                        MethodInfo toLowerMethod = typeof(string).GetMethod("ToLowerInvariant");

                        var expression1 = Expression.Call(body, toLowerMethod);
                        
                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                                
                        return Expression.Call(expression1, method, constantExpression);
                    }
                case OperatorEnum.ContainsCaseSensitive:
                    {
                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                        return Expression.Call(body, method, constantExpression);
                    }
                case OperatorEnum.GreaterThan:
                    {


                        return Expression.GreaterThan(body, constantExpression);
                    }
                case OperatorEnum.LessThan:
                    {
                        return Expression.LessThan(body, constantExpression);
                    }
                case OperatorEnum.GreaterOrEqual:
                    {
                        return Expression.GreaterThanOrEqual(body, constantExpression);
                    }
                case OperatorEnum.LessOrEqual:
                    {
                        return Expression.LessThanOrEqual(body, constantExpression);
                    }
                case OperatorEnum.NotEquals:
                    {
                        return Expression.NotEqual(body, constantExpression);
                    }
            }
        }
        #endregion

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
