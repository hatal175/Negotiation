using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negotiation.Models
{
    public static class IEnumerableUtils
    {
        private static IEnumerable<T> SingleItemAsEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item)
        {
            return source.Except(item.SingleItemAsEnumerable());
        }
        
    }
}