using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NET.AutoApi;
using NET.AutoWebApi.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace NET.AutoWebApi
{
    public class AutoApiConventionalControllerFeatureProvider : ControllerFeatureProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public AutoApiConventionalControllerFeatureProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override bool IsController(TypeInfo typeInfo)
        {

            var configuration = _serviceProvider
                .GetRequiredService<IOptions<AutoApiConventionalControllerOptions>>().Value
                .GetConventionalControllerSettingOrNull(typeInfo.AsType());

            if (configuration == null)
            {
                return false;
            }
            return true;
        }
    }
}
