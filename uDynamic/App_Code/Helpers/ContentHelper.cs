using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Umbraco.Core.Models;

using uDynamic.Extensions;


namespace uDynamic.Helpers
{
    public class ContentHelper
    {
        public static string GetPropertyValueAsString(object content, string propertyAlias)
        {
            string result = string.Empty;
            if (content is IContent)
            {
                result = ((IContent)content).GetPropertyValueAsString(propertyAlias);
            }
            else if (content is IPublishedContent)
            {
                result = ((IPublishedContent)content).GetPropertyValueAsString(propertyAlias);
            }
            return result;
        }
    }

}