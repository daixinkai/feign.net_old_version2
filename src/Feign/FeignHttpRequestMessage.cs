using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignHttpRequestMessage : HttpRequestMessage
    {
        public FeignHttpRequestMessage(FeignClientRequest feignClientRequest)
        {
            FeignClientRequest = feignClientRequest;
        }

        public FeignHttpRequestMessage(FeignClientRequest feignClientRequest, HttpMethod method, string requestUri) : base(method, requestUri)
        {
            FeignClientRequest = feignClientRequest;
        }

        public FeignHttpRequestMessage(FeignClientRequest feignClientRequest, HttpMethod method, Uri requestUri) : base(method, requestUri)
        {
            FeignClientRequest = feignClientRequest;
        }

        public FeignClientRequest FeignClientRequest { get; }

    }
}
