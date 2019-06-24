using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Feign
{
    public sealed class SendingRequestEventArgs : FeignClientEventArgs
    {
        internal SendingRequestEventArgs(IFeignClient feignClient, FeignHttpRequestMessage requestMessage) : base(feignClient)
        {
            RequestMessage = requestMessage;
        }
        public FeignHttpRequestMessage RequestMessage { get; }

        public bool IsSuspendRequest => _isSuspendRequest;

        bool _isSuspendRequest;

        public void SuspendRequest()
        {
            _isSuspendRequest = true;
        }

    }
}
