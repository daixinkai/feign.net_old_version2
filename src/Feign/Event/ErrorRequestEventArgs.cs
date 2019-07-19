using Feign.Request;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public sealed class ErrorRequestEventArgs : FeignClientEventArgs
    {
        internal ErrorRequestEventArgs(IFeignClient feignClient, Exception exception) : base(feignClient)
        {
            Exception = exception;
            if (exception is FeignHttpRequestException)
            {
                RequestMessage = ((FeignHttpRequestException)exception).RequestMessage;
            }
        }
        internal ErrorRequestEventArgs(IFeignClient feignClient, FeignHttpRequestMessage requestMessage, Exception exception) : base(feignClient)
        {
            RequestMessage = requestMessage;
            Exception = exception;
        }
        public FeignHttpRequestMessage RequestMessage { get; }
        public Exception Exception { get; }
        public bool ExceptionHandled { get; set; }
    }
}
