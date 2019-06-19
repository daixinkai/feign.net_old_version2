using Feign;
using Feign.Cache;
using Feign.Logging;
using Feign.Proxy;
using Feign.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceCollectionExtensions
    {

        public static IFeignBuilder AddFeignClients(this IServiceCollection services)
        {
            return AddFeignClients(services, (FeignOptions)null);
        }

        public static IFeignBuilder AddFeignClients(this IServiceCollection services, Action<FeignOptions> setupAction)
        {
            FeignOptions options = new FeignOptions();
            setupAction?.Invoke(options);
            return AddFeignClients(services, options);
        }

        public static IFeignBuilder AddFeignClients(this IServiceCollection services, FeignOptions options)
        {
            if (options == null)
            {
                options = new FeignOptions();
            }

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton<IServiceCacheProvider, ServiceCacheProvider>();

            FeignBuilder feignBuilder = new FeignBuilder();

            feignBuilder.Services = services;
            feignBuilder.Options = options;

            if (options.Assemblies.Count == 0)
            {
                AddFeignClients(feignBuilder.FeignClientTypeBuilder, services, Assembly.GetEntryAssembly(), options.Lifetime);
            }
            else
            {
                foreach (var assembly in options.Assemblies)
                {
                    AddFeignClients(feignBuilder.FeignClientTypeBuilder, services, assembly, options.Lifetime);
                }
            }
            services.TryAddSingleton<IFeignOptions>(options);
            feignBuilder.FeignClientTypeBuilder.FinishBuild();
            return feignBuilder;
        }

        static void AddFeignClients(FeignClientTypeBuilder feignClientTypeBuilder, IServiceCollection services, Assembly assembly, ServiceLifetime lifetime)
        {
            if (assembly == null)
            {
                return;
            }
            foreach (var serviceType in assembly.GetTypes())
            {
                Type proxyType = feignClientTypeBuilder.BuildType(serviceType);
                if (proxyType == null)
                {
                    continue;
                }
                AddService(services, lifetime, serviceType, proxyType);
                // add fallback
                FeignClientAttribute feignClientAttribute = serviceType.GetCustomAttribute<FeignClientAttribute>();

                if (feignClientAttribute.Fallback != null)
                {
                    AddService(services, lifetime, feignClientAttribute.Fallback);
                }
                if (feignClientAttribute.FallbackFactory != null)
                {
                    AddService(services, lifetime, feignClientAttribute.FallbackFactory);
                }
            }
        }


        static void AddService(IServiceCollection services, ServiceLifetime lifetime, Type serviceType, Type implType)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.TryAddSingleton(serviceType, implType);
                    break;
                case ServiceLifetime.Scoped:
                    services.TryAddScoped(serviceType, implType);
                    break;
                case ServiceLifetime.Transient:
                    services.TryAddTransient(serviceType, implType);
                    break;
                default:
                    break;
            }
        }
        static void AddService(IServiceCollection services, ServiceLifetime lifetime, Type type)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.TryAddSingleton(type);
                    break;
                case ServiceLifetime.Scoped:
                    services.TryAddScoped(type);
                    break;
                case ServiceLifetime.Transient:
                    services.TryAddTransient(type);
                    break;
                default:
                    break;
            }
        }
    }
}
