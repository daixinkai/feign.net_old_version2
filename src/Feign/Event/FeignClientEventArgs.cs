using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public abstract class FeignClientEventArgs : EventArgs
    {
        protected FeignClientEventArgs(IFeignClient feignClient)
        {
            FeignClient = feignClient;
        }
        public IFeignClient FeignClient { get; }
    }

    //public abstract class FeignClientEventArgs<TService> : EventArgs
    //{
    //    protected FeignClientEventArgs(IFeignClient<TService> feignClient)
    //    {
    //        FeignClient = feignClient;
    //    }
    //    public IFeignClient<TService> FeignClient { get; }
    //}
}
