using Feign;
using Feign.Cache;
using Feign.Discovery;
using Feign.Formatting;
using Feign.Logging;
using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FeignBuilderExtensions
    {
        public static IFeignBuilder AddConverter<TSource, TResult>(this IFeignBuilder feignBuilder, IConverter<TSource, TResult> converter)
        {
            feignBuilder.Options.Converters.AddConverter(converter);
            return feignBuilder;
        }

        public static void AddFeignClients(this IFeignBuilder feignBuilder, Assembly assembly, FeignClientScope scope)
        {
            if (assembly == null)
            {
                return;
            }
            foreach (var serviceType in assembly.GetTypes())
            {
                Type proxyType = feignBuilder.TypeBuilder.BuildType(serviceType);
                if (proxyType == null)
                {
                    continue;
                }
                feignBuilder.AddService(serviceType, proxyType, scope);
                // add fallback
                FeignClientAttribute feignClientAttribute = serviceType.GetCustomAttribute<FeignClientAttribute>();
                if (feignClientAttribute.Fallback != null)
                {
                    feignBuilder.AddService(feignClientAttribute.Fallback, scope);
                }
                if (feignClientAttribute.FallbackFactory != null)
                {
                    feignBuilder.AddService(feignClientAttribute.FallbackFactory, scope);
                }
            }
        }

        public static void AddLoggerFactory<TLoggerFactory>(this IFeignBuilder feignBuilder) where TLoggerFactory : ILoggerFactory
        {
            feignBuilder.AddService(typeof(ILoggerFactory), typeof(TLoggerFactory), FeignClientScope.Singleton);
        }

        public static void AddServiceDiscovery<TServiceDiscovery>(this IFeignBuilder feignBuilder) where TServiceDiscovery : IServiceDiscovery
        {
            feignBuilder.AddService(typeof(IServiceDiscovery), typeof(TServiceDiscovery), FeignClientScope.Singleton);
        }

        public static void AddServiceCacheProvider<TServiceCacheProvider>(this IFeignBuilder feignBuilder) where TServiceCacheProvider : IServiceCacheProvider
        {
            feignBuilder.AddService(typeof(IServiceCacheProvider), typeof(TServiceCacheProvider), FeignClientScope.Singleton);
        }

        //public static void AddMissingDefaultServices(this IFeignBuilder feignBuilder)
        //{
        //    if (!feignBuilder.IsRegister(typeof(IServiceCacheProvider)))
        //    {
        //        feignBuilder.AddServiceCacheProvider<DefaultServiceCacheProvider>();
        //    }
        //    if (!feignBuilder.IsRegister(typeof(IServiceDiscovery)))
        //    {
        //        feignBuilder.AddServiceDiscovery<DefaultServiceDiscovery>();
        //    }
        //    if (!feignBuilder.IsRegister(typeof(ILoggerFactory)))
        //    {
        //        feignBuilder.AddLoggerFactory<DefaultLoggerFactory>();
        //    }
        //}

    }
}