using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NET.AutoWebApi.Helper;

/// <summary>
/// Type帮助类
/// </summary>
public static class TypeHelper
{

    /// <summary>
    /// 不为null的基本类型集合
    /// </summary>
    private static readonly HashSet<Type> NonNullablePrimitiveTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(bool),
            typeof(float),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        };

    /// <summary>
    /// 是否基本类型，不包含可空基本类型
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNonNullablePrimitiveType(Type type)
    {
        return NonNullablePrimitiveTypes.Contains(type);
    }


    /// <summary>
    /// 判断类型type是否是基本类型，（包含了支持可空基本类型判断）
    /// </summary>
    /// <param name="type"></param>
    /// <param name="includeNullables"></param>
    /// <param name="includeEnums"></param>
    /// <returns></returns>
    public static bool IsPrimitiveExtended(Type type, bool includeNullables = true, bool includeEnums = false)
    {
        if (IsPrimitiveExtendedInternal(type, includeEnums))
        {
            return true;
        }

        //如果是可空的基本类型，则取出泛型参数
        if (includeNullables && IsNullable(type) && type.GenericTypeArguments.Any())
        {
            return IsPrimitiveExtendedInternal(type.GenericTypeArguments[0], includeEnums);
        }

        return false;
    }

    public static bool IsNullable(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// 是否是基本类型，
    /// 或者是：string、decimal、DateTime、DateTimeOffset、TimeSpan、Guid
    /// </summary>
    /// <param name="type"></param>
    /// <param name="includeEnums">基本类型判断：是否包含枚举类型</param>
    /// <returns></returns>
    private static bool IsPrimitiveExtendedInternal(Type type, bool includeEnums)
    {
        if (type.IsPrimitive)
        {
            return true;
        }

        if (includeEnums && type.IsEnum)
        {
            return true;
        }

        return type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }


}
