using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class GenericExtensions
    {
        /// <summary>
        /// 当前对象是否在集合中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsIn<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }
        /// <summary>
        /// 当前对象是否在集合中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsIn<T>(this T item, IEnumerable<T> list)
        {
            return list.Contains(item);
        }
    }
}
