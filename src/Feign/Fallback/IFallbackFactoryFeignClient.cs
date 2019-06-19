using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Fallback
{
    public interface IFallbackFactoryFeignClient</*out*/ T> : IFeignClient 
    {
        IFallbackFactory<T> FallbackFactory { get; }
    }
}
