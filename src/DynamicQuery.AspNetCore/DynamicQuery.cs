using DynamicQuery.Parser;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DynamicQuery.AspNetCore
{
    [ModelBinder(BinderType = typeof(DynamicQueryBinder))]
    public partial class DynamicQuery<T> : DynamicQueryModel<T>
    {
    }
}
