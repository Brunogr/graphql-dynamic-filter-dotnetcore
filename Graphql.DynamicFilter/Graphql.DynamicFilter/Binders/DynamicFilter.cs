using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Graphql.DynamicFiltering
{
    [ModelBinder(BinderType = typeof(DynamicFilterBinder))]
    public class DynamicFilter<T>
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        public Expression<Func<T, object>> Order { get; set; }
        public OrderType OrderType { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public Type ItemType => typeof(T);
    }

    public enum OrderType
    {
        Asc = 1,
        Desc = 2
    }
}
