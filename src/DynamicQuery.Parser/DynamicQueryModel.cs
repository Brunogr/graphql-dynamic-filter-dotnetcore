using System;
using System.Linq.Expressions;

namespace DynamicQuery.Parser
{
    public class DynamicQueryModel<T>
    {
        public Expression<Func<T, bool>>? Filter { get; set; }
        public Expression<Func<T, object>>? Order { get; set; }
        public Expression<Func<T, T>>? Select { get; set; }
        public string? SelectText { get; set; }
        public OrderType OrderType { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public Type ItemType => typeof(T);
    }
}
