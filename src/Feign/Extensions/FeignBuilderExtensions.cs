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

        public static TFeignBuilder AddFeignClients<TFeignBuilder>(this TFeignBuilder feignBuilder, FeignOptions options) where TFeignBuilder : IFeignBuilder
        {
            if (options.Assemblies.Count == 0)
            {
                feignBuilder.AddFeignClients(Assembly.GetEntryAssembly(), options.Lifetime);
            }
            else
            {
                foreach (var assembly in options.Assemblies)
                {
                    feignBuilder.AddFeignClients(assembly, options.Lifetime);
                }
            }
            feignBuilder.AddLoggerFactory<DefaultLoggerFactory>();
            feignBuilder.AddServiceCacheProvider<DefaultServiceCacheProvider>();
            feignBuilder.AddServiceDiscovery<DefaultServiceDiscovery>();
            feignBuilder.AddService<IFeignOptions>(options);
            feignBuilder.TypeBuilder.FinishBuild();
            return feignBuilder;
        }

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
            feignBuilder.AddOrUpdateService(typeof(ILoggerFactory), typeof(TLoggerFactory), FeignClientLifetime.Singleton);
            return feignBuilder;
        }


        public static IFeignBuilder AddServiceDiscovery<TServiceDiscovery>(this IFeignBuilder feignBuilder) where TServiceDiscovery : IServiceDiscovery
        {
            feignBuilder.AddOrUpdateService(typeof(IServiceDiscovery), typeof(TServiceDiscovery), FeignClientLifetime.Singleton);
            return feignBuilder;
        }

        public static IFeignBuilder AddServiceCacheProvider<TServiceCacheProvider>(this IFeignBuilder feignBuilder) where TServiceCacheProvider : IServiceCacheProvider
        {
            feignBuilder.AddOrUpdateService(typeof(IServiceCacheProvider), typeof(TServiceCacheProvider), FeignClientLifetime.Singleton);
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