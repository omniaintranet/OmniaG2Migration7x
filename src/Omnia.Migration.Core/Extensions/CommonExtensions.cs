using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Extensions
{
    public static class CommonExtensions
    {
        public static bool IsNullOrEmpty(this Guid? guid)
        {
            return guid == null || guid.Value == Guid.Empty;
        }
    }
}
