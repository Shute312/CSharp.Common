using System;
using System.Linq;
using System.Linq.Expressions;

namespace Common.ExtensionMethods
{
    public static class IQueryableEx
    {
        /// <summary>
        /// 如果满足condition，则执行Where；反之，不执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> queryable, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? queryable.Where(predicate) : queryable;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="skipCount"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        public static IQueryable<T> Paged<T>(this IQueryable<T> queryable, int skipCount, int takeCount)
        {
            IQueryable<T> q = queryable;
            if (skipCount > 0)
            {
                q = q.Skip(skipCount);
            }
            if (takeCount > 0)
            {
                q = q.Take(takeCount);
            }
            return q;
        }
    }
}
