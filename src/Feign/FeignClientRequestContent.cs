using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public abstract class FeignClientRequestContent
    {
        public abstract HttpContent GetHttpContent(MediaTypeHeaderValue contentType);
    }
}
