using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public interface IHttpImageClient
    {
        ValueTask<byte[]> GetImage(string imageUrl);
    }
}
