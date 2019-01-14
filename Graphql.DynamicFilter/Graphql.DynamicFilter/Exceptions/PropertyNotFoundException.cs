using System;
using System.Collections.Generic;
using System.Text;

namespace Graphql.DynamicFilter.Exceptions
{
    [Serializable]
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(string propertyName, string className) : base($"Property {propertyName} was not found on {className}")
        {

        }
    }
}
