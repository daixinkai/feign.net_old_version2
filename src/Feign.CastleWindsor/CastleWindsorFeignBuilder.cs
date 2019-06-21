using System;
using System.Collections.Generic;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Feign.Formatting;
using Feign.Reflection;

namespace Feign.CastleWindsor
{
    sealed class CastleWindsorFeignBuilder : ICastleWindsorFeignBuilder
    {

        public CastleWindsorFeignBuilder()
        {
            TypeBuilder = new FeignClientTypeBuilder();
        }

        public FeignOptions Options { get; set; }

        public IWindsorContainer WindsorContainer { get; set; }

        public FeignClientTypeBuilder TypeBuilder { get; }

        IFeignOptions IFeignBuilder.Options => Options;

        public void AddService(Type serviceType, Type implType, FeignClientLifetime lifetime)
        {
            var registration = Component.For(serviceType).ImplementedBy(implType);
            WindsorContainer.Register(Lifestyle(registration, lifetime));
        }
        public void AddService(Type serviceType, FeignClientLifetime lifetime)
        {
            var registration = Component.For(serviceType);
            WindsorContainer.Register(Lifestyle(registration, lifetime));
        }
        public void AddService<TService>(TService service) where TService : class
        {
            var registration = Component.For<TService>().Instance(service);
            WindsorContainer.Register(registration);
        }

        public bool HasService(Type serviceType)
        {
            return WindsorContainer.Kernel.HasComponent(serviceType);
        }

        public void RemoveService(Type serviceType)
        {
            WindsorContainer.Kernel.RemoveComponent(serviceType);
        }

        ComponentRegistration<T> Lifestyle<T>(ComponentRegistration<T> registration, FeignClientLifetime lifetime)
    where T : class
        {
            switch (lifetime)
            {
                case FeignClientLifetime.Transient:
                    return registration.LifestyleTransient();
                case FeignClientLifetime.Singleton:
                    return registration.LifestyleSingleton();
                case FeignClientLifetime.Scoped:
                    return registration.LifestyleScoped();
                default:
                    return registration;
            }
        }


    }
}
