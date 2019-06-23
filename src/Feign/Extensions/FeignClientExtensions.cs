using Feign.Fallback;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FeignClientExtensions
    {
        public static IFallbackFeignClient AsFallback(this IFeignClient feignClient)
        {
            return feignClient as IFallbackFeignClient;
        }
        public static IFallbackFeignClient<TService> AsFallback<TService>(this IFeignClient feignClient)
        {
            return feignClient as IFallbackFeignClient<TService>;
        }

        public static object GetFallback(this IFeignClient feignClient)
        {
            return feignClient.AsFallback()?.Fallback;
        }
        public static TService GetFallback<TService>(this IFeignClient feignClient)
        {
            IFallbackFeignClient<TService> fallbackFeignClient = feignClient.AsFallback<TService>();
            if (fallbackFeignClient == null)
            {
                return default(TService);
            }
            return fallbackFeignClient.Fallback;
        }

    }
}
