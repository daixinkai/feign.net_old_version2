using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Formatting
{
    public interface IMediaTypeFormatter
    {
        string MediaType { get; }
        HttpContent GetHttpContent(object content, MediaTypeHeaderValue contentType);
        TResult GetResult<TResult>(byte[] buffer, Encoding encoding);
    }
}
