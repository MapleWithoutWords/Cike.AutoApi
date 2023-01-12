using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NET.AutoWebApi.Consts;
using NET.AutoWebApi.Helper;
using NET.AutoWebApi.Options;
using NET.AutoWebApi.Setting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

namespace NET.AutoWebApi
{
    public class AutoApiServiceConvention : IAutoApiServiceConvention
    {
        public ILogger<AutoApiServiceConvention> Logger { get; set; }

        protected AutoApiConventionalControllerOptions Options { get; }
        protected IConventionalRouteBuilder ConventionalRouteBuilder { get; }

        public AutoApiServiceConvention(
            IOptions<AutoApiConventionalControllerOptions> options, IConventionalRouteBuilder conventionalRouteBuilder)
        {
            Options = options.Value;

            Logger = NullLogger<AutoApiServiceConvention>.Instance;
            ConventionalRouteBuilder = conventionalRouteBuilder;
        }



        public void Apply(ApplicationModel application)
        {
            RemoveDuplicateControllers(application);

            foreach (var controller in GetControllers(application))
            {
                var controllerType = controller.ControllerType.AsType();

                //拿到自动api的配置
                var configuration = GetControllerSettingOrNull(controllerType);


                if (ImplementsAutoApiServiceInterface(controllerType))
                {
                    controller.ControllerName = controller.ControllerName.RemovePostFix(Options.RemoveServiceNameFix.ToArray());
                    configuration?.ControllerModelConfigurer?.Invoke(controller);
                    ConfigureAutoApiService(controller, configuration);
                }
                else
                {
                    var remoteServiceAttr = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(controllerType.GetTypeInfo());
                    if (remoteServiceAttr != null && remoteServiceAttr.IsEnabled)
                    {
                        ConfigureAutoApiService(controller, configuration);
                    }
                }
            }
        }



        protected virtual IList<ControllerModel> GetControllers(ApplicationModel application)
        {
            return application.Controllers;
        }

        /// <summary>
        /// 删除重复的控制器
        /// </summary>
        /// <param name="application"></param>
        protected virtual void RemoveDuplicateControllers(ApplicationModel application)
        {
            var removeControllerModelList = new List<ControllerModel>();

            foreach (var controllerModel in GetControllers(application))
            {

                var baseControllerTypes = GetBaseClasseList(controllerModel.ControllerType, typeof(Controller))
                    .Where(t => !t.IsAbstract)
                    .ToArray();

                if (baseControllerTypes.Length == 0)
                {
                    continue;
                }

                var baseControllerModelList = GetControllers(application)
                    .Where(cm => baseControllerTypes.Contains(cm.ControllerType))
                    .ToArray();

                if (baseControllerModelList.Length == 0)
                {
                    continue;
                }

                removeControllerModelList.Add(controllerModel);
            }

            foreach (var controllerModel in removeControllerModelList)
            {
                application.Controllers.Remove(controllerModel);
            }
        }

        /// <summary>
        /// 获取类型 type的所有父类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stoppingType">如果父类是该类型则停止</param>
        /// <returns></returns>
        public Type[] GetBaseClasseList(Type type, Type stoppingType)
        {
            List<Type> list = new List<Type>();
            AddTypeAndBaseTypesRecursively(list, type.BaseType, stoppingType);
            return list.ToArray();
        }
        private void AddTypeAndBaseTypesRecursively(List<Type> types, Type type, Type stoppingType = null)
        {
            if (type != null && type != stoppingType && type != typeof(object))
            {
                AddTypeAndBaseTypesRecursively(types, type.BaseType, stoppingType);
                types.Add(type);
            }
        }

        protected virtual void ConfigureAutoApiService(ControllerModel controller, AutoApiConventionalControllerSetting configuration)
        {
            ConfigureApiExplorer(controller);
            ConfigureSelector(controller, configuration);
            ConfigureParameters(controller);
        }

        protected virtual void ConfigureParameters(ControllerModel controller)
        {
            /* Default binding system of Asp.Net Core for a parameter
             * 1. Form values
             * 2. Route values.
             * 3. Query string.
             */

            foreach (var action in controller.Actions)
            {
                foreach (var prm in action.Parameters)
                {
                    if (prm.BindingInfo != null)
                    {
                        continue;
                    }

                    if (!TypeHelper.IsPrimitiveExtended(prm.ParameterInfo.ParameterType, includeEnums: true))
                    {
                        if (CanUseFormBodyBinding(action, prm))
                        {
                            prm.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
                        }
                    }
                }
            }
        }

        protected virtual bool CanUseFormBodyBinding(ActionModel action, ParameterModel parameter)
        {
            //We want to use "id" as path parameter, not body!
            if (parameter.ParameterName == "id")
            {
                return false;
            }

            if (Options
                .FormBodyBindingIgnoredTypes
                .Any(t => t.IsAssignableFrom(parameter.ParameterInfo.ParameterType)))
            {
                return false;
            }

            foreach (var selector in action.Selectors)
            {
                if (selector.ActionConstraints == null)
                {
                    continue;
                }

                foreach (var actionConstraint in selector.ActionConstraints)
                {
                    var httpMethodActionConstraint = actionConstraint as HttpMethodActionConstraint;
                    if (httpMethodActionConstraint == null)
                    {
                        continue;
                    }

                    if (httpMethodActionConstraint.HttpMethods.All(hm => hm.IsIn("GET", "DELETE", "TRACE", "HEAD")))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 配置控制器
        /// </summary>
        /// <param name="controller"></param>
        protected virtual void ConfigureApiExplorer(ControllerModel controller)
        {
            if (controller.ApiExplorer.GroupName.IsNullOrEmpty())
            {
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }

            if (controller.ApiExplorer.IsVisible == null)
            {
                controller.ApiExplorer.IsVisible = IsVisibleAutoApiService(controller.ControllerType);
            }

            foreach (var action in controller.Actions)
            {
                ConfigureApiActionExplorer(action);
            }
        }

        /// <summary>
        /// 配置Action
        /// </summary>
        /// <param name="action"></param>
        protected virtual void ConfigureApiActionExplorer(ActionModel action)
        {
            if (action.ApiExplorer.IsVisible != null)
            {
                return;
            }

            var visible = IsVisibleAutoApiServiceMethod(action.ActionMethod);
            if (visible == null)
            {
                return;
            }

            action.ApiExplorer.IsVisible = visible;
        }

        protected virtual void ConfigureSelector(ControllerModel controller, AutoApiConventionalControllerSetting configuration)
        {
            //删除空的路由选择器
            RemoveEmptySelectors(controller.Selectors);

            var controllerType = controller.ControllerType.AsType();
            var remoteServiceAtt = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(controllerType.GetTypeInfo()); 
            if (remoteServiceAtt != null && !remoteServiceAtt.IsEnabled)
            {
                return;
            }

            //如果已经配置了路由信息，则不配置
            if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
            {
                return;
            }

            //获取跟路由
            var rootPath = GetRootPathOrDefault(controller.ControllerType.AsType());

            foreach (var action in controller.Actions)
            {
                ConfigureSelector(rootPath, controller.ControllerName, action, configuration);
            }
        }

        /// <summary>
        /// 配置每个action的路由信息
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="controllerName"></param>
        /// <param name="action"></param>
        /// <param name="configuration"></param>
        protected virtual void ConfigureSelector(string rootPath, string controllerName, ActionModel action, AutoApiConventionalControllerSetting configuration)
        {
            //删除空的路由选择器
            RemoveEmptySelectors(action.Selectors);

            var remoteServiceAtt = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(action.ActionMethod);
            if (remoteServiceAtt != null && !remoteServiceAtt.IsEnabled)
            {
                return;
            }

            if (!action.Selectors.Any())
            {
                AddAutoApiServiceSelector(rootPath, controllerName, action, configuration);
            }
            else
            {
                NormalizeSelectorRoutes(rootPath, controllerName, action, configuration);
            }
        }
        /// <summary>
        /// 添加自动api配置路由选择器
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="controllerName"></param>
        /// <param name="action"></param>
        /// <param name="configuration"></param>
        protected virtual void AddAutoApiServiceSelector(string rootPath, string controllerName, ActionModel action, AutoApiConventionalControllerSetting configuration)
        {
            //获取http方法
            var httpMethod = SelectHttpMethod(action, configuration);

            var abpServiceSelectorModel = new SelectorModel
            {
                AttributeRouteModel = CreateAutoApiServiceAttributeRouteModel(rootPath, controllerName, action, httpMethod, configuration),
                ActionConstraints = { new HttpMethodActionConstraint(new[] { httpMethod }) }
            };

            action.Selectors.Add(abpServiceSelectorModel);
        }

        protected virtual string SelectHttpMethod(ActionModel action, AutoApiConventionalControllerSetting configuration)
        {
            return HttpMethodHelper.GetConventionalVerbForMethodName(action.ActionName);
        }

        /// <summary>
        /// 配置空的路由选择器为自动api的路由配置
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="controllerName"></param>
        /// <param name="action"></param>
        /// <param name="configuration"></param>
        protected virtual void NormalizeSelectorRoutes(string rootPath, string controllerName, ActionModel action, AutoApiConventionalControllerSetting configuration)
        {
            foreach (var selector in action.Selectors)
            {
                var httpMethod = selector.ActionConstraints
                    .OfType<HttpMethodActionConstraint>()
                    .FirstOrDefault()?
                    .HttpMethods?
                    .FirstOrDefault();

                if (httpMethod == null)
                {
                    httpMethod = SelectHttpMethod(action, configuration);
                }

                if (selector.AttributeRouteModel == null)
                {
                    selector.AttributeRouteModel = CreateAutoApiServiceAttributeRouteModel(rootPath, controllerName, action, httpMethod, configuration);
                }

                if (!selector.ActionConstraints.OfType<HttpMethodActionConstraint>().Any())
                {
                    selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { httpMethod }));
                }
            }
        }

        /// <summary>
        /// 获取路由根路径或默认根路径
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        protected virtual string GetRootPathOrDefault(Type controllerType)
        {
            var controllerSetting = GetControllerSettingOrNull(controllerType);
            if (controllerSetting?.RootPath != null)
            {
                return controllerSetting.RootPath;
            }

            var areaAttribute = controllerType.GetCustomAttributes().OfType<AreaAttribute>().FirstOrDefault();
            if (areaAttribute?.RouteValue != null)
            {
                return areaAttribute.RouteValue;
            }

            return AutoApiConsts.DefaultRootPath;
        }


        protected virtual AutoApiConventionalControllerSetting GetControllerSettingOrNull(Type controllerType)
        {
            return Options.GetConventionalControllerSettingOrNull(controllerType);
        }

        /// <summary>
        /// 创建路由信息
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="controllerName"></param>
        /// <param name="action"></param>
        /// <param name="httpMethod"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        protected virtual AttributeRouteModel CreateAutoApiServiceAttributeRouteModel(string rootPath, string controllerName, ActionModel action, string httpMethod, AutoApiConventionalControllerSetting configuration)
        {
            return new AttributeRouteModel(
                new RouteAttribute(
                    ConventionalRouteBuilder.Build(rootPath, controllerName, action, httpMethod, configuration)
                )
            );
        }

        protected virtual void RemoveEmptySelectors(IList<SelectorModel> selectors)
        {
            selectors
                .Where(IsEmptySelector)
                .ToList()
                .ForEach(s => selectors.Remove(s));
        }

        protected virtual bool IsEmptySelector(SelectorModel selector)
        {
            return selector.AttributeRouteModel == null
                   && selector.ActionConstraints.IsNullOrEmpty()
                   && selector.EndpointMetadata.IsNullOrEmpty();
        }

        /// <summary>
        /// 类型是否继承了 IAutoApiService 接口
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        protected virtual bool ImplementsAutoApiServiceInterface(Type controllerType)
        {
            return typeof(IAutoApiService).GetTypeInfo().IsAssignableFrom(controllerType);
        }

        protected virtual bool IsVisibleAutoApiService(Type controllerType)
        {

            var attribute = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(controllerType);
            if (attribute == null)
            {
                return true;
            }

            return attribute.IsEnabled;
        }

        /// <summary>
        /// 是否显示自动api方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        protected virtual bool? IsVisibleAutoApiServiceMethod(MethodInfo method)
        {
            
            var attribute = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(method);
            if (attribute == null)
            {
                return null;
            }

            return attribute.IsEnabled;
        }

    }
}
