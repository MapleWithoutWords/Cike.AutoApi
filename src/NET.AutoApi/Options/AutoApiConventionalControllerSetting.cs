using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NET.AutoWebApi.Setting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NET.AutoWebApi.Options
{
    /// <summary>
    /// 自动api转换为控制器配置
    /// </summary>
    public class AutoApiConventionalControllerSetting
    {
        public Assembly Assembly { get; }

        public HashSet<Type> ControllerTypes { get; }

        public string RootPath { get; set; }


        public Func<Type, bool> TypePredicate { get; set; }

        public Action<ControllerModel> ControllerModelConfigurer { get; set; }

        public AutoApiConventionalControllerSetting(Assembly assembly,string rootPath)
        {
            Assembly = assembly;
            RootPath = rootPath;

            ControllerTypes = new HashSet<Type>();
        }

        public void Initialize()
        {
            var types = Assembly.GetTypes()
                .Where(IsRemoteService);
            //.WhereIf(TypePredicate != null, TypePredicate)

            foreach (var type in types)
            {
                ControllerTypes.Add(type);
            }
        }

        private static bool IsRemoteService(Type type)
        {
            if (!type.IsPublic || type.IsAbstract || type.IsGenericType)
            {
                return false;
            }

            var remoteServiceAttr = type.GetCustomAttribute<AutoApiAttribute>();
            if (remoteServiceAttr != null && !remoteServiceAttr.IsEnabled)
            {
                return false;
            }

            if (typeof(IAutoApiService).IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }
    }
}
