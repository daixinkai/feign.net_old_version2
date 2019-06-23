using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Fallback
{
    public interface IFallbackFeignClient : IFeignClient
    {
        object Fallback { get; }
    }
    public interface IFallbackFeignClient<out T> : IFallbackFeignClient
    {
        new T Fallback { get; }
    }
}
