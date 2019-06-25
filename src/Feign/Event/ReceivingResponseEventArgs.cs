using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public class ReceivingResponseEventArgs : FeignClientEventArgs
    {
        internal ReceivingResponseEventArgs(IFeignClient feignClient, HttpResponseMessage responseMessage, Type resultType) : base(feignClient)
        {
            ResponseMessage = responseMessage;
            ResultType = resultType;
        }
        public HttpResponseMessage ResponseMessage { get; }

        public Type ResultType { get; }

        internal bool _isSetResult;

        object _result;

        public object Result
        {
            get
            {
                return _result;
            }
            set
            {
                _isSetResult = true;
                _result = value;
            }
        }

        internal T GetResult<T>()
        {
            return Result == null ? default(T) : (T)Result;
        }

    }

    public sealed class ReceivingResponseEventArgs<T> : ReceivingResponseEventArgs
    {
        internal ReceivingResponseEventArgs(IFeignClient feignClient, HttpResponseMessage responseMessage) : base(feignClient, responseMessage, typeof(T))
        {
        }
    }
}
