using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Caching;
using System.Web.Routing;

namespace Ricettario.Controllers.Abstract
{
    public static class Extensions
    {
        public static string ToLowerOrEmpty(this string str)
        {
            if (str == null)
            {
                return String.Empty;
            }
            return str.ToLowerInvariant();
        }

        public static string JoinStrings(this IEnumerable<string> list, string separator = "")
        {
            if (list == null)
            {
                return String.Empty;
            }
            return string.Join(separator, list);
        }

        public static T QuickCache<T>(this MemoryCache cache, string key, Func<T> factory)
        {
            var lazyObject = new Lazy<T>(factory);
            var returnedLazyObject = cache.AddOrGetExisting(key, lazyObject,new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(1)});
            return ((Lazy<T>)returnedLazyObject).Value;
        }

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }
    }
}