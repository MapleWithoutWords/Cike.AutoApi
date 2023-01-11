using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NET.AutoWebApi.Options;

namespace NET.AutoWebApi;

public interface IConventionalRouteBuilder
{
    /// <summary>
    /// 构建路由地址
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="controllerName"></param>
    /// <param name="action"></param>
    /// <param name="httpMethod"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    string Build(
        string rootPath,
        string controllerName,
        ActionModel action,
        string httpMethod,
         AutoApiConventionalControllerSetting configuration
    );
}
