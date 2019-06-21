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

        public static TFeignBuilder AddFeignClients<TFeignBuilder>(this TFeignBuilder feignBuilder, Assembly assembly, FeignClientLifetime lifetime)
            where TFeignBuilder : IFeignBuilder
        {
            if (assembly == null)
            {
                return feignBuilder;
            }
            foreach (var serviceType in assembly.GetTypes())
            {
                Type proxyType = feignBuilder.TypeBuilder.BuildType(serviceType);
                if (proxyType == null)
                {
                    continue;
                }
                feignBuilder.AddService(serviceType, proxyType, lifetime);
                // add fallback
                FeignClientAttribute feignClientAttribute = serviceType.GetCustomAttribute<FeignClientAttribute>();
                if (feignClientAttribute.Fallback != null)
                {
                    feignBuilder.AddService(feignClientAttribute.Fallback, lifetime);
                }
                if (feignClientAttribute.FallbackFactory != null)
                {
                    feignBuilder.AddService(feignClientAttribute.FallbackFactory, lifetime);
                }
            }
            return feignBuilder;
        }

        public static IFeignBuilder AddLoggerFactory<TLoggerFactory>(this IFeignBuilder feignBuilder) where TLoggerFactory : ILoggerFactory
        {
            if (feignBuilder.HasService(typeof(ILoggerFactory)))
            {
                feignBuilder.RemoveService(typeof(ILoggerFactory));
            }
            feignBuilder.AddService(typeof(ILoggerFactory), typeof(TLoggerFactory), FeignClientLifetime.Singleton);
            return feignBuilder;
        }


        public static IFeignBuilder AddServiceDiscovery<TServiceDiscovery>(this IFeignBuilder feignBuilder) where TServiceDiscovery : IServiceDiscovery
        {
            if (feignBuilder.HasService(typeof(IServiceDiscovery)))
            {
                feignBuilder.RemoveService(typeof(IServiceDiscovery));
            }
            feignBuilder.AddService(typeof(IServiceDiscovery), typeof(TServiceDiscovery), FeignClientLifetime.Singleton);
            return feignBuilder;
        }

        public static IFeignBuilder AddServiceCacheProvider<TServiceCacheProvider>(this IFeignBuilder feignBuilder) where TServiceCacheProvider : IServiceCacheProvider
        {
            if (feignBuilder.HasService(typeof(IServiceCacheProvider)))
            {
                feignBuilder.RemoveService(typeof(IServiceCacheProvider));
            }
            feignBuilder.AddService(typeof(IServiceCacheProvider), typeof(TServiceCacheProvider), FeignClientLifetime.Singleton);
            return feignBuilder;
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