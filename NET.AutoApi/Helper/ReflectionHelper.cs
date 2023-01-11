using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NET.AutoWebApi.Helper;

/// <summary>
/// 反射帮助类
/// </summary>
public static class ReflectionHelper
{

    /// <summary>
    /// 获取唯一的Attribute或默认值
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="memberInfo"></param>
    /// <param name="defaultValue"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static TAttribute GetSingleAttributeOrDefault<TAttribute>(MemberInfo memberInfo, TAttribute defaultValue = default, bool inherit = true)
        where TAttribute : Attribute
    {
        if (memberInfo.IsDefined(typeof(TAttribute), inherit))
        {
            return memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>().First();
        }

        return defaultValue;
    }


}
