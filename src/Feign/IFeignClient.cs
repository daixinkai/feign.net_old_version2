using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IFeignClient
    {
        /// <summary>
        /// Gets the serviceId
        /// </summary>
        string ServiceId { get; }
    }
}
