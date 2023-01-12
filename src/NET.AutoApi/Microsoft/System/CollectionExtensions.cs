using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace System.Collections.Generic
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// 检查集合是否为null或空
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        /// 向集合中添加元素，如果存在则不添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {

            if (source.Contains(item))
            {
                return false;
            }

            source.Add(item);
            return true;
        }
    }
}
