using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Feign.Formatting;
using Feign.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Feign.DependencyInjection
{
    sealed class DependencyInjectionFeignBuilder : IDependencyInjectionFeignBuilder
    {

        public DependencyInjectionFeignBuilder()
        {
            TypeBuilder = new FeignClientTypeBuilder();
        }

        public ConverterCollection Converters { get { return Options?.Converters; } }

        public FeignOptions Options { get; set; }

        public IServiceCollection Services { get; set; }

        public FeignClientTypeBuilder TypeBuilder { get; }

        IFeignOptions IFeignBuilder.Options => Options;


        public void AddService(Type serviceType, Type implType, FeignClientScope scope)
        {
            switch (scope)
            {
                case FeignClientScope.Singleton:
                    Services.TryAddSingleton(serviceType, implType);
                    break;
                case FeignClientScope.Scoped:
                    Services.TryAddScoped(serviceType, implType);
                    break;
                case FeignClientScope.Transient:
                    Services.TryAddTransient(serviceType, implType);
                    break;
                default:
                    break;
            }
        }
        public void AddService(Type serviceType, FeignClientScope scope)
        {
            switch (scope)
            {
                case FeignClientScope.Singleton:
                    Services.TryAddSingleton(serviceType);
                    break;
                case FeignClientScope.Scoped:
                    Services.TryAddScoped(serviceType);
                    break;
                case FeignClientScope.Transient:
                    Services.TryAddTransient(serviceType);
                    break;
                default:
                    break;
            }
        }

        public void AddService<TService>(TService service) where TService : class
        {
            Services.AddSingleton<TService>(service);
        }

        public bool IsRegister(Type serviceType)
        {
            return Services.Any(a => a.ServiceType == serviceType);
        }
    }
}
