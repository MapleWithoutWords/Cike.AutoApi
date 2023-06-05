using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Abp.AutoWebApi.Options
{
    public class AutoApiConventionalControllerOptions
    {
        public string DefaultRootPath { get; set; } = "";
        public string DefaultApiPrefix { get; set; } = "api";
        /// <summary>
        /// 控制器转换配置
        /// </summary>
        public List<AutoApiConventionalControllerSetting> ConventionalControllerSettings { get; }

        /// <summary>
        /// 获取包含该类型的转换配置
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public AutoApiConventionalControllerSetting GetConventionalControllerSettingOrNull(Type controllerType)
        {
            return this.ConventionalControllerSettings.FirstOrDefault(controllerSetting => controllerSetting.ControllerTypes.Contains(controllerType));
        }

        /// <summary>
        /// 请求报文体模型绑定忽略类型
        /// </summary>
        public List<Type> FormBodyBindingIgnoredTypes { get; }
        /// <summary>
        /// 删除自动api的多余名称
        /// </summary>
        public List<string> RemoveServiceNameFix { get; }


        public AutoApiConventionalControllerOptions()
        {
            ConventionalControllerSettings = new List<AutoApiConventionalControllerSetting>();

            FormBodyBindingIgnoredTypes = new List<Type>
            {
                typeof(IFormFile),
                typeof(IRemoteStreamContent)
            };
            RemoveServiceNameFix = new List<string>
            {
                "Service"
            };
        }

        /// <summary>
        /// 创建一个自动api控制器转换配置
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public AutoApiConventionalControllerOptions CreateConventional(Assembly assembly, Action<AutoApiConventionalControllerSetting> optionsAction = null)
        {
            var setting = new AutoApiConventionalControllerSetting(
                assembly,
                this.DefaultRootPath
            );

            optionsAction?.Invoke(setting);
            setting.Initialize();
            ConventionalControllerSettings.Add(setting);
            return this;
        }
        /// <summary>
        /// 创建一个自动api控制器转换配置
        /// </summary>
        /// <param name="assemblies"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public AutoApiConventionalControllerOptions CreateConventional(Assembly[] assemblies, Action<AutoApiConventionalControllerSetting> optionsAction = null)
        {
            foreach (var assembly in assemblies)
            {
                var setting = new AutoApiConventionalControllerSetting(
                    assembly,
                    this.DefaultRootPath
                );

                optionsAction?.Invoke(setting);
                setting.Initialize();
                ConventionalControllerSettings.Add(setting);
            }
            return this;
        }
    }
}
