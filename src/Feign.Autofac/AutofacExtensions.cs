using Feign;
using Feign.Autofac;
using Feign.Cache;
using Feign.Discovery;
using Feign.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Autofac
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AutofacExtensions
    {
        public static IAutofacFeignBuilder AddFeignClients(this ContainerBuilder containerBuilder)
        {
            return AddFeignClients(containerBuilder, (FeignOptions)null);
        }

        public static IAutofacFeignBuilder AddFeignClients(this ContainerBuilder containerBuilder, Action<FeignOptions> setupAction)
        {
            FeignOptions options = new FeignOptions();
            setupAction?.Invoke(options);
            return AddFeignClients(containerBuilder, options);
        }

        public static IAutofacFeignBuilder AddFeignClients(this ContainerBuilder containerBuilder, FeignOptions options)
        {
            if (options == null)
            {
                options = new FeignOptions();
            }


            AutofacFeignBuilder feignBuilder = new AutofacFeignBuilder();

            feignBuilder.ContainerBuilder = containerBuilder;
            feignBuilder.Options = options;
            feignBuilder.AddServiceCacheProvider<DefaultServiceCacheProvider>();
            feignBuilder.AddServiceDiscovery<DefaultServiceDiscovery>();
            feignBuilder.AddLoggerFactory<DefaultLoggerFactory>();

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
            feignBuilder.AddService<IFeignOptions>(options);
            feignBuilder.TypeBuilder.FinishBuild();
            return feignBuilder;
        }

    }
}
