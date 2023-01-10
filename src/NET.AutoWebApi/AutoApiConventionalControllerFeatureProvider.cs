using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NET.AutoWebApi
{
    public class AutoApiConventionalControllerFeatureProvider : ControllerFeatureProvider
    {

        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo);
        }
    }
}
