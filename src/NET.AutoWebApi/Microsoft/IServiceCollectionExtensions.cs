using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NET.AutoWebApi;
using NET.AutoWebApi.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static void AddAutoApiService(this IServiceCollection services, Action<MvcOptions> mvcOptionAction = null, Action<AutoApiConventionalControllerOptions> optionAction = null)
        {
            services.Configure<MvcOptions>(opt =>
            {
                opt.Conventions.Add(new AutoApiServiceConventionWrapper(services));
                mvcOptionAction?.Invoke(opt);
            });
            services.Configure<AutoApiConventionalControllerOptions>(opt =>
            {
                optionAction?.Invoke(opt);
            });

            services.AddSingleton<IConventionalRouteBuilder, ConventionalRouteBuilder>();
            services.AddTransient<IAutoApiServiceConvention, AutoApiServiceConvention>();
        }


        public static void UseAutoApiService(this IHost host, params Assembly[] assemblies)
        {
            var partManager = host.Services.GetService<ApplicationPartManager>();
            partManager.FeatureProviders.Add(new AutoApiConventionalControllerFeatureProvider(host));
            var conventionalOptions = host.Services.GetService<IOptions<AutoApiConventionalControllerOptions>>();


            foreach (var moduleAssembly in conventionalOptions.Value.ConventionalControllerSettings)
            {
                partManager.ApplicationParts.AddIfNotContains(moduleAssembly.Assembly);
            }
        }

        public static void AddIfNotContains(this IList<ApplicationPart> applicationParts, Assembly assembly)
        {
            if (applicationParts.Any(
                p => p is AssemblyPart assemblyPart && assemblyPart.Assembly == assembly))
            {
                return;
            }

            applicationParts.Add(new AssemblyPart(assembly));
        }
    }
}
