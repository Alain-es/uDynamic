﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Umbraco.Core;

namespace uDynamic.Extensions
{
    internal static class IListExtensions
    {
        public static void AddRangeUnique<T>(this IList<T> self, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (!self.Contains(item))
                {
                    self.Add(item);
                }
            }
        }

        public static void AddRangeUniqueWithTrimming(this IList<string> self, IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                if (!self.Contains(item.Trim()))
                {
                    self.Add(item.Trim());
                }
            }
        }

    }

}