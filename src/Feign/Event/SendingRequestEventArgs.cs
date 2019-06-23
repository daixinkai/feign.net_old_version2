using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public sealed class SendingRequestEventArgs : FeignClientEventArgs
    {
        internal SendingRequestEventArgs(IFeignClient feignClient, HttpRequestMessage requestMessage) : base(feignClient)
        {
            RequestMessage = requestMessage;
        }
        public HttpRequestMessage RequestMessage { get; }

        public bool IsSuspendRequest => _isSuspendRequest;

        bool _isSuspendRequest;

        public void SuspendRequest()
        {
            _isSuspendRequest = true;
        }

    }
}
