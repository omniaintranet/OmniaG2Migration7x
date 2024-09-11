using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Helpers
{
    public static class Logger
    {
        public static List<string> Logs { get; set; }

        public static void Log(string message)
        {
            Logs.Add(message);
        }

        static Logger()
        {
            Logs = new List<string>();
        }
    }
}
