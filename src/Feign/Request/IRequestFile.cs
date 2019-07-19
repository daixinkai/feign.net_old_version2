using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Request
{
    public interface IRequestFile
    {
        string Name { get; }
        HttpContent GetHttpContent();
    }
}
