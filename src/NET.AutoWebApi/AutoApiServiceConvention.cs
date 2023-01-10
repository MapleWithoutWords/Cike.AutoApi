using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NET.AutoWebApi.Options;
using NET.AutoWebApi.Setting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace NET.AutoWebApi
{
    public class AutoApiServiceConvention : IAutoApiServiceConvention
    {
        public ILogger<AutoApiServiceConvention> Logger { get; set; }

        protected AutoApiConventionalControllerOptions Options { get; }

        public AutoApiServiceConvention(
            IOptions<AutoApiConventionalControllerOptions> options)
        {
            Options = options.Value;

            Logger = NullLogger<AutoApiServiceConvention>.Instance;
        }



        public void Apply(ApplicationModel application)
        {
            ApplyForControllers(application);
        }

        protected virtual void ApplyForControllers(ApplicationModel application)
        {
            RemoveDuplicateControllers(application);

            foreach (var controller in GetControllers(application))
            {
                var controllerType = controller.ControllerType.AsType();

                var configuration = GetControllerSettingOrNull(controllerType);


                if (ImplementsRemoteServiceInterface(controllerType))
                {
                    foreach (var item in Options.RemoveServiceNameFix)
                    {
                        controller.ControllerName = controller.ControllerName.Replace(item, "");
                    }
                    configuration?.ControllerModelConfigurer?.Invoke(controller);
                    ConfigureRemoteService(controller, configuration);
                }
                else
                {
                    var controllerTypeInfo = controllerType.GetTypeInfo();

                    if (controllerTypeInfo.IsDefined(typeof(AutoApiAttribute), true)==false)
                    {
                        continue;
                    }
                    var remoteServiceAttr = controllerTypeInfo.GetCustomAttributes(typeof(AutoApiAttribute), true).Cast<AutoApiAttribute>().First();
                    if (remoteServiceAttr != null && remoteServiceAttr.IsEnabled)
                    {
                        ConfigureRemoteService(controller, configuration);
                    }
                }
            }
        }


        protected virtual IList<ControllerModel> GetControllers(ApplicationModel application)
        {
            return application.Controllers;
        }

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

        protected virtual void ConfigureRemoteService(ControllerModel controller, AutoApiConventionalControllerSetting configuration)
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

        protected virtual void ConfigureApiExplorer(ControllerModel controller)
        {
            if (controller.ApiExplorer.GroupName.IsNullOrEmpty())
            {
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }

            if (controller.ApiExplorer.IsVisible == null)
            {
                controller.ApiExplorer.IsVisible = IsVisibleRemoteService(controller.ControllerType);
            }

            foreach (var action in controller.Actions)
            {
                ConfigureApiExplorer(action);
            }
        }

        protected virtual void ConfigureApiExplorer(ActionModel action)
        {
            if (action.ApiExplorer.IsVisible != null)
            {
                return;
            }

            var visible = IsVisibleRemoteServiceMethod(action.ActionMethod);
            if (visible == null)
            {
                return;
            }

            action.ApiExplorer.IsVisible = visible;
        }

        protected virtual void ConfigureSelector(ControllerModel controller, AutoApiConventionalControllerSetting configuration)
        {
            RemoveEmptySelectors(controller.Selectors);

            var controllerType = controller.ControllerType.AsType();
            var remoteServiceAtt = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(controllerType.GetTypeInfo());
            if (remoteServiceAtt != null && !remoteServiceAtt.IsEnabledFor(controllerType))
            {
                return;
            }

            if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
            {
                return;
            }

            var rootPath = GetRootPathOrDefault(controller.ControllerType.AsType());

            foreach (var action in controller.Actions)
            {
                ConfigureSelector(rootPath, controller.ControllerName, action, configuration);
            }
        }

        protected virtual void ConfigureSelector(string rootPath, string controllerName, ActionModel action, AutoApiConventionalControllerSetting configuration)
        {
            RemoveEmptySelectors(action.Selectors);

            var remoteServiceAtt = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(action.ActionMethod);
            if (remoteServiceAtt != null && !remoteServiceAtt.IsEnabledFor(action.ActionMethod))
            {
                return;
            }

            if (!action.Selectors.Any())
            {
                AddAbpServiceSelector(rootPath, controllerName, action, configuration);
            }
            else
            {
                NormalizeSelectorRoutes(rootPath, controllerName, action, configuration);
            }
        }

        protected virtual void AddAbpServiceSelector(string rootPath, string controllerName, ActionModel action, [CanBeNull] AutoApiConventionalControllerSetting configuration)
        {
            var httpMethod = SelectHttpMethod(action, configuration);

            var abpServiceSelectorModel = new SelectorModel
            {
                AttributeRouteModel = CreateAbpServiceAttributeRouteModel(rootPath, controllerName, action, httpMethod, configuration),
                ActionConstraints = { new HttpMethodActionConstraint(new[] { httpMethod }) }
            };

            action.Selectors.Add(abpServiceSelectorModel);
        }

        protected virtual string SelectHttpMethod(ActionModel action, AutoApiConventionalControllerSetting configuration)
        {
            return HttpMethodHelper.GetConventionalVerbForMethodName(action.ActionName);
        }

        protected virtual void NormalizeSelectorRoutes(string rootPath, string controllerName, ActionModel action, [CanBeNull] AutoApiConventionalControllerSetting configuration)
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
                    selector.AttributeRouteModel = CreateAbpServiceAttributeRouteModel(rootPath, controllerName, action, httpMethod, configuration);
                }

                if (!selector.ActionConstraints.OfType<HttpMethodActionConstraint>().Any())
                {
                    selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { httpMethod }));
                }
            }
        }

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

            return ModuleApiDescriptionModel.DefaultRootPath;
        }


        protected virtual AutoApiConventionalControllerSetting GetControllerSettingOrNull(Type controllerType)
        {
            return Options.ConventionalControllerSettings.GetSettingOrNull(controllerType);
        }

        protected virtual AttributeRouteModel CreateAbpServiceAttributeRouteModel(string rootPath, string controllerName, ActionModel action, string httpMethod, AutoApiConventionalControllerSetting configuration)
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

        protected virtual bool ImplementsRemoteServiceInterface(Type controllerType)
        {
            return typeof(IAutoApiService).GetTypeInfo().IsAssignableFrom(controllerType);
        }

        protected virtual bool IsVisibleRemoteService(Type controllerType)
        {

            var attribute = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(controllerType);
            if (attribute == null)
            {
                return true;
            }

            return attribute.IsEnabledFor(controllerType) &&
                   attribute.IsMetadataEnabledFor(controllerType);
        }

        protected virtual bool? IsVisibleRemoteServiceMethod(MethodInfo method)
        {
            var attribute = ReflectionHelper.GetSingleAttributeOrDefault<AutoApiAttribute>(method);
            if (attribute == null)
            {
                return null;
            }

            return attribute.IsEnabledFor(method) &&
                   attribute.IsMetadataEnabledFor(method);
        }

    }
}
