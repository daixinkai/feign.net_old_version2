using Feign;
using Feign.Discovery;
using Feign.Formatting;
using Feign.Proxy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Feign.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FeignBuilderExtensions
    {

        public static IDependencyInjectionFeignBuilder AddServiceDiscovery(this IDependencyInjectionFeignBuilder feignBuilder, IServiceDiscovery serviceDiscovery)
        {
            feignBuilder.Services.TryAddSingleton(serviceDiscovery);
            return feignBuilder;
        }

        public static IDependencyInjectionFeignBuilder AddServiceDiscovery<T>(this IDependencyInjectionFeignBuilder feignBuilder) where T : class, IServiceDiscovery
        {
            feignBuilder.Services.TryAddSingleton<IServiceDiscovery, T>();
            return feignBuilder;
        }


    }
}