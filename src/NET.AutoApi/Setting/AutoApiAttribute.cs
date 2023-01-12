using System;
using System.Collections.Generic;
using System.Text;

namespace NET.AutoWebApi.Setting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface)]
    public class AutoApiAttribute:Attribute
    {

        public bool IsEnabled { get; set; }
        public AutoApiAttribute(bool isEnabled = true)
        {
            IsEnabled = isEnabled;
        }
    }
}
