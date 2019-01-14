using Graphql.DynamicFilter.Exceptions;
using System;
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
            var values = DefineOperation(filterValues, itemType);

            Value = ParseValue(values[1]);
        }

        #region [ Properties ]
        public PropertyInfo Property { get; set; }
        public object Value { get; set; }
        public OperatorEnum Condition { get; set; }

        #endregion

        #region [ Private Methods ]
        private object ParseValue(string value)
        {
            object parsedValue = null;

            //Verifying if is nullable
            if (Nullable.GetUnderlyingType(Property.PropertyType) != null)
            {
                var underlyingType = Nullable.GetUnderlyingType(Property.PropertyType);
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
                parsedValue = ChangeType(value, Property.PropertyType);
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

            Property = itemType.GetProperty(values[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

            if (Property == null)
                throw new PropertyNotFoundException(values[0], itemType.Name);

            return values;
        }

        #endregion

        #region [ Public methods ]
        public Expression GetExpression(ParameterExpression parameter)
        {
            var constantExpression = Expression.Constant(Value);

            //making constant nullable
            if (Nullable.GetUnderlyingType(Property.PropertyType) != null)
            {
                var type = typeof(Nullable<>).MakeGenericType(Nullable.GetUnderlyingType(Property.PropertyType));
                constantExpression = Expression.Constant(Value, type);
            }

            switch (Condition)
            {
                default:
                case OperatorEnum.Equals:
                    {
                        return Expression.Equal(Expression.Property(parameter, Property), constantExpression);
                    }
                case OperatorEnum.Contains:
                    {
                        constantExpression = Expression.Constant(Value.ToString().ToLower());

                        var property = Expression.Property(parameter, Property);
                        
                        MethodInfo toLowerMethod = typeof(string).GetMethod("ToLowerInvariant");

                        var expression1 = Expression.Call(property, toLowerMethod);
                        
                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                                
                        return Expression.Call(expression1, method, constantExpression);
                    }
                case OperatorEnum.ContainsCaseSensitive:
                    {
                        var property = Expression.Property(parameter, Property);

                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                        return Expression.Call(property, method, constantExpression);
                    }
                case OperatorEnum.GreaterThan:
                    {
                        return Expression.GreaterThan(Expression.Property(parameter, Property), constantExpression);
                    }
                case OperatorEnum.LessThan:
                    {
                        return Expression.LessThan(Expression.Property(parameter, Property), constantExpression);
                    }
                case OperatorEnum.GreaterOrEqual:
                    {
                        return Expression.GreaterThanOrEqual(Expression.Property(parameter, Property), constantExpression);
                    }
                case OperatorEnum.LessOrEqual:
                    {
                        return Expression.LessThanOrEqual(Expression.Property(parameter, Property), constantExpression);
                    }
                case OperatorEnum.NotEquals:
                    {
                        return Expression.NotEqual(Expression.Property(parameter, Property), constantExpression);
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
