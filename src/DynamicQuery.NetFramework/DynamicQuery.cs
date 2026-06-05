using DynamicQuery.Parser;
using System;
using System.Web.Mvc;

namespace DynamicQuery.NetFramework
{
    [ModelBinder(typeof(DynamicQueryBinder))]
    public class DynamicQuery<T> : DynamicQueryModel<T>
    {
    }
}
