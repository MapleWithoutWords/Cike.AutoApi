using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace NET.AutoWebApi.Helper;

/// <summary>
/// http方法帮助类
/// </summary>
public static class HttpMethodHelper
{
    public const string DefaultHttpVerb = "POST";

    public static Dictionary<string, string[]> ConventionalPrefixes { get; set; } = new Dictionary<string, string[]>
        {
            {"GET", new[] {"GetList", "GetAll", "Get"}},
            {"PUT", new[] {"Put", "Update"}},
            {"DELETE", new[] {"Delete", "Remove"}},
            {"POST", new[] {"Create", "Add", "Insert", "Post"}},
            {"PATCH", new[] {"Patch"}}
        };

    /// <summary>
    /// 获取methodName的http方法
    /// </summary>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static string GetConventionalVerbForMethodName(string methodName)
    {
        foreach (var conventionalPrefix in ConventionalPrefixes)
        {
            if (conventionalPrefix.Value.Any(prefix => methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return conventionalPrefix.Key;
            }
        }

        return DefaultHttpVerb;
    }

    /// <summary>
    /// 删除http方法前缀
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="httpMethod"></param>
    /// <returns></returns>
    public static string RemoveHttpMethodPrefix(string methodName, string httpMethod)
    {

        var prefixes = ConventionalPrefixes.TryGetValue(httpMethod, out string[] obj) ? obj : default;
        if (prefixes.IsNullOrEmpty())
        {
            return methodName;
        }
        foreach (var item in prefixes)
        {
            methodName = methodName.Replace(item, "");
        }
        return methodName;
    }

    /// <summary>
    /// 转换http方法
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static HttpMethod ConvertToHttpMethod(string httpMethod)
    {
        switch (httpMethod.ToUpperInvariant())
        {
            case "GET":
                return HttpMethod.Get;
            case "POST":
                return HttpMethod.Post;
            case "PUT":
                return HttpMethod.Put;
            case "DELETE":
                return HttpMethod.Delete;
            case "OPTIONS":
                return HttpMethod.Options;
            case "TRACE":
                return HttpMethod.Trace;
            case "HEAD":
                return HttpMethod.Head;
            case "PATCH":
                return new HttpMethod("PATCH");
            default:
                throw new ArgumentException("Unknown HTTP METHOD: " + httpMethod);
        }
    }
}
