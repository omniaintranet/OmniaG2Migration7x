using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Helpers
{
    public static class JsonHelper
    {
        public static JToken SafeJTokenParse(string jsonStr)
        {
            try
            {
                return JToken.Parse(jsonStr);
            }
            catch (JsonReaderException)
            {
                return JToken.FromObject(jsonStr);
            }
        }
    }
}
