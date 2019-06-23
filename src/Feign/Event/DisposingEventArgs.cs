using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class DisposingEventArgs : FeignClientEventArgs
    {
        public DisposingEventArgs(IFeignClient feignClient, bool disposing) : base(feignClient)
        {
            Disposing = disposing;
        }
        public bool Disposing { get; }
    }
}
