using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        private readonly IHost _host;

        public AutoApiConventionalControllerFeatureProvider(IHost host)
        {
            _host = host;
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            if (_host.Services == null)
            {
                return false;
            }

            var configuration = _host.Services
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
