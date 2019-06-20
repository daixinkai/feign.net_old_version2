using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Autofac.Builder;
using Feign.Formatting;
using Feign.Reflection;

namespace Feign.Autofac
{
    sealed class AutofacFeignBuilder : IAutofacFeignBuilder
    {

        public AutofacFeignBuilder()
        {
            TypeBuilder = new FeignClientTypeBuilder();
        }

        public ConverterCollection Converters { get { return Options?.Converters; } }

        public FeignOptions Options { get; set; }

        public ContainerBuilder ContainerBuilder { get; set; }

        public FeignClientTypeBuilder TypeBuilder { get; }

        IFeignOptions IFeignBuilder.Options => Options;

        public void AddService(Type serviceType, Type implType, FeignClientScope scope)
        {
            var registerBuilder = ContainerBuilder.RegisterType(implType).As(serviceType);
            switch (scope)
            {
                case FeignClientScope.Singleton:
                    registerBuilder.SingleInstance();
                    break;
                case FeignClientScope.Scoped:
                    registerBuilder.InstancePerLifetimeScope();
                    break;
                case FeignClientScope.Transient:
                    registerBuilder.InstancePerDependency();
                    break;
                default:
                    break;
            }
        }
        public void AddService(Type serviceType, FeignClientScope scope)
        {
            var registerBuilder = ContainerBuilder.RegisterType(serviceType);
            switch (scope)
            {
                case FeignClientScope.Singleton:
                    registerBuilder.SingleInstance();
                    break;
                case FeignClientScope.Scoped:
                    registerBuilder.InstancePerLifetimeScope();
                    break;
                case FeignClientScope.Transient:
                    registerBuilder.InstancePerDependency();
                    break;
                default:
                    break;
            }
        }
        public void AddService<TService>(TService service) where TService : class
        {
            ContainerBuilder.RegisterInstance(service).As<TService>();
        }

        public bool IsRegister(Type serviceType)
        {
            throw new NotSupportedException();
            //IContainer container = ContainerBuilder.Build();
            //using (container)
            //{
            //    return container.IsRegistered(serviceType);
            //}
        }

    }
}
