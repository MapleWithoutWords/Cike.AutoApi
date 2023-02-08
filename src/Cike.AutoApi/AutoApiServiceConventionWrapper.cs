using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Cike.AutoApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cike.AutoWebApi
{
    public class AutoApiServiceConventionWrapper : IAutoApiServiceConvention
    {
        private readonly IAutoApiServiceConvention _convention;

        public AutoApiServiceConventionWrapper(IServiceProvider serviceProvider)
        {
            _convention = serviceProvider.GetRequiredService<IAutoApiServiceConvention>();
        }

        public void Apply(ApplicationModel application)
        {
            _convention.Apply(application);
        }
    }
}
