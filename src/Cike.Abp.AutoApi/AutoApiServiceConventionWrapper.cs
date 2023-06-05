using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Abp.AutoApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.AutoWebApi
{
    public class AutoApiServiceConventionWrapper : IRemoteServiceConvention
    {
        private readonly IRemoteServiceConvention _convention;

        public AutoApiServiceConventionWrapper(IServiceProvider serviceProvider)
        {
            _convention = serviceProvider.GetRequiredService<IRemoteServiceConvention>();
        }

        public void Apply(ApplicationModel application)
        {
            _convention.Apply(application);
        }
    }
}
