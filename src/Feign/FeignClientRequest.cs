using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignClientRequest
    {
        public FeignClientRequest(string baseUrl, string mappingUri, string uri, string method, string mediaType, object content)
        {
            BaseUrl = baseUrl;
            MappingUri = mappingUri;
            Uri = uri;
            Method = method;
            MediaType = mediaType;
            Content = content;
        }
        public string BaseUrl { get; }
        public string MappingUri { get; }
        public string Uri { get; }
        public string Method { get; }
        public string MediaType { get; }
        public object Content { get; }
    }
}
