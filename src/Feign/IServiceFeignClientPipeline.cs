﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public interface IServiceFeignClientPipeline : IFeignClientPipeline
    {
    }

    public interface IServiceFeignClientPipeline<TService> : IServiceFeignClientPipeline
    {
    }

}
