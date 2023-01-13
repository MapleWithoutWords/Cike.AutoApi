using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using NET.AutoWebApi.Consts;
using NET.AutoWebApi.Helper;
using NET.AutoWebApi.Options;

namespace NET.AutoWebApi;

public class ConventionalRouteBuilder : IConventionalRouteBuilder
{
    protected AutoApiConventionalControllerOptions Options { get; }

    public ConventionalRouteBuilder(IOptions<AutoApiConventionalControllerOptions> options)
    {
        Options = options.Value;
    }

    /// <summary>
    /// 构建路由地址
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="controllerName"></param>
    /// <param name="action"></param>
    /// <param name="httpMethod"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public virtual string Build(
        string rootPath,
        string controllerName,
        ActionModel action,
        string httpMethod,
        AutoApiConventionalControllerSetting configuration)
    {
        var apiRoutePrefix = AutoApiConsts.DefaultApiPrefix;
        var controllerNameInUrl = controllerName;

        var url = $"{apiRoutePrefix}/{rootPath}/{controllerName.ToKebabCase()}".Replace("//","/");

        var idParameterModel = action.Parameters.FirstOrDefault(p => p.ParameterName == "id");
        if (idParameterModel != null)
        {
            if (TypeHelper.IsPrimitiveExtended(idParameterModel.ParameterType, includeEnums: true))
            {
                url += "/{id}";
            }
            else
            {
                var properties = idParameterModel
                    .ParameterType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var property in properties)
                {
                    url += "/{" + property.Name + "}";
                }
            }
        }

        //配置方法名称及参数
        var actionNameInUrl = HttpMethodHelper.RemoveHttpMethodPrefix(action.ActionName, httpMethod).RemovePostFix("Async");
        if (!actionNameInUrl.IsNullOrEmpty())
        {
            url += $"/{actionNameInUrl.ToKebabCase()}";

            //添加其他id参数到路由地址
            var secondaryIds = action.Parameters
                .Where(p => p.ParameterName.EndsWith("Id", StringComparison.Ordinal)).ToList();
            if (secondaryIds.Count == 1)
            {
                url += $"/{{{secondaryIds[0].ParameterName}}}";
            }
        }

        return url;
    }

}