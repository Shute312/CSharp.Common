using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ExtensionMethods
{
    public static class EnumerableEx
    {
        public static bool IsNeitherNullNorEmpty(this string str)
        {
            return str != null && str.Length > 0;
        }
        public static bool IsNullOrEmpty(this string str)
        {
            return str == null || str.Length == 0;
        }
        /// <summary>
        /// 去重复
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector)
        {
            return source.Distinct(new CommonEqualityComparer<T, V>(keySelector));
        }

        /// <summary>
        /// null 或者空(长度为0)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null) return true;
            return source.Count() == 0;
        }
        /// <summary>
        /// 非空
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNeitherNullNorEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null) return false;
            return source.Count() > 0;
        }

        /// <summary>
        /// 安全的获取数量的方法(null返回0)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int CountSafe<T>(this IEnumerable<T> source)
        {
            if (source == null) return 0;
            return source.Count();
        }

        public static IEnumerable<TResult> SelectSafe<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source.IsNeitherNullNorEmpty()) return source.Select(selector);
            return null;
        }
        public static IEnumerable<TSource> AsEnumerableSafe<TSource>(this IEnumerable<TSource> source)
        {
            if (source.IsNeitherNullNorEmpty()) return source.AsEnumerable();
            return null;
        }
        public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            if (source.IsNeitherNullNorEmpty()) return source.ToDictionary(keySelector, elementSelector);
            return null;
        }
        public static List<TSource> ToListSafe<TSource>(this IEnumerable<TSource> source)
        {
            if (source.IsNeitherNullNorEmpty()) return source.ToList();
            return null;
        }

        public static T GetValueSafe<T>(this Dictionary<string, T> dic, string key)
        {
            if (dic == null) return default(T); ;
            if (key.IsNullOrEmpty()) return default(T);
            T value = default(T);
            if (dic.TryGetValue(key, out value)) return value;
            else return default(T);
        }
        //ref http://blog.csdn.net/shaopengfei/article/details/36426763
        public class CommonEqualityComparer<T, V> : IEqualityComparer<T>
        {
            private Func<T, V> keySelector;

            public CommonEqualityComparer(Func<T, V> keySelector)
            {
                this.keySelector = keySelector;
            }

            public bool Equals(T x, T y)
            {
                return EqualityComparer<V>.Default.Equals(keySelector(x), keySelector(y));
            }

            public int GetHashCode(T obj)
            {
                return EqualityComparer<V>.Default.GetHashCode(keySelector(obj));
            }
        }
    }
}
