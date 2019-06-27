using Feign.Internal;
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
    public class JsonMediaTypeFormatter : IMediaTypeFormatter
    {
        public JsonMediaTypeFormatter()
        {
            MediaType = "application/json";
        }
        public string MediaType { get; }

        public HttpContent GetHttpContent(object content, MediaTypeHeaderValue contentType)
        {
            if (content == null)
            {
                return null;
            }
            return new ObjectStringContent(content);
        }

        public TResult GetResult<TResult>(byte[] buffer, Encoding encoding)
        {
            string json = (encoding ?? Encoding.Default).GetString(buffer);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TResult>(json);
        }


    }
}
