using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignHttpRequestException : HttpRequestException
    {
        public FeignHttpRequestException(IFeignClient feignClient, FeignHttpRequestMessage requestMessage, HttpRequestException httpRequestException) : base(GetMessage(feignClient, requestMessage, httpRequestException))
        {
            FeignClient = feignClient;
            RequestMessage = requestMessage;
        }

        public IFeignClient FeignClient { get; }
        public FeignHttpRequestMessage RequestMessage { get; }

        static string GetMessage(IFeignClient feignClient, FeignHttpRequestMessage httpRequestMessage, HttpRequestException httpRequestException)
        {
            return $"{httpRequestMessage.Method.Method} request error on {httpRequestMessage.RequestUri.ToString()} : {httpRequestException.Message}";
        }

    }
}
