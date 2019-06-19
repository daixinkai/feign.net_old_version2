﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Feign
{
    public sealed class CancelRequestEventArgs : FeignClientEventArgs
    {
        internal CancelRequestEventArgs(IFeignClient feignClient, CancellationToken cancellationToken) : base(feignClient)
        {
            CancellationToken = cancellationToken;
        }
        public CancellationToken CancellationToken { get; }
    }
}
