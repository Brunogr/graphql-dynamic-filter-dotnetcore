using System;
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

                var newValue = underlyingType.IsEnum ? Convert.ChangeType(Enum.Parse(underlyingType, value), underlyingType) : Convert.ChangeType(value, underlyingType);

                var nullableObject = Activator.CreateInstance(type, newValue);
                
                parsedValue = nullableObject;
            }
            else
                parsedValue = Property.PropertyType.IsEnum ? Convert.ChangeType(Enum.Parse(Property.PropertyType, value), Property.PropertyType) : Convert.ChangeType(value, Property.PropertyType);

            return parsedValue;
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
                values = filterValues.Split('%');
                Condition = OperatorEnum.Contains;
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

            Property = itemType.GetProperty(values[0]);

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
        GreaterThan = 3,
        LessThan = 4,
        GreaterOrEqual = 5,
        LessOrEqual = 6,
        NotEquals = 7

    }
}
