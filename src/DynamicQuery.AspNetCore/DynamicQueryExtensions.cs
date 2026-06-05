using DynamicQuery.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicQuery.AspNetCore
{
    public static class DynamicQueryExtensions
    {
        public static IEnumerable<T> Apply<T>(this IEnumerable<T> source, DynamicQueryModel<T> query)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            IEnumerable<T> result = source;

            if (query.Filter != null)
                result = result.Where(query.Filter.Compile());

            if (query.Select != null)
                result = result.Select(query.Select.Compile());

            if (query.Order != null)
            {
                result = query.OrderType == OrderType.Asc
                    ? result.OrderBy(query.Order.Compile())
                    : result.OrderByDescending(query.Order.Compile());
            }

            if (query.PageSize > 0)
                result = result.Skip(query.Page).Take(query.PageSize);

            return result;
        }

        public static IQueryable<T> Apply<T>(this IQueryable<T> source, DynamicQueryModel<T> query)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            IQueryable<T> result = source;

            if (query.Filter != null)
                result = result.Where(query.Filter);

            if (query.Select != null)
                result = result.Select(query.Select);

            if (query.Order != null)
            {
                result = query.OrderType == OrderType.Asc
                    ? OrderBy(result, query.Order)
                    : OrderByDescending(result, query.Order);
            }

            if (query.PageSize > 0)
                result = result.Skip(query.Page).Take(query.PageSize);

            return result;
        }

        private static IOrderedQueryable<T> OrderBy<T>(IQueryable<T> source, Expression<Func<T, object>> keySelector)
        {
            return Queryable.OrderBy(source, keySelector);
        }

        private static IOrderedQueryable<T> OrderByDescending<T>(IQueryable<T> source, Expression<Func<T, object>> keySelector)
        {
            return Queryable.OrderByDescending(source, keySelector);
        }
    }
}
