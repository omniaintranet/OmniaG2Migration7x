using Omnia.Fx.Models.Apps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Extensions
{
    public static class AppInstanceExtensions
    {
        public static bool HasSPUrl(this AppInstance appInstance, string url)
        {
            //Hieu rem
            //if (!string.IsNullOrEmpty(url) && appInstance.Properties.Properties.ContainsKey("spPath"))
            //{
            //    return url.ToLower().EndsWith(appInstance.Properties.Properties["spPath"]?.ToString().ToLower());
            //}
            if (!string.IsNullOrEmpty(url))
            {
                return url.ToLower() ==url.ToLower();
            }


            else
                return false;
            
        }
    }
}
