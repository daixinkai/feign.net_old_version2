﻿using System;
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

        public IFeignOptions Options { get; set; }

        public IServiceCollection Services { get; set; }

        public IFeignClientTypeBuilder TypeBuilder { get; }

        public void AddService(Type serviceType, Type implType, FeignClientLifetime lifetime)
        {
            switch (lifetime)
            {
                case FeignClientLifetime.Singleton:
                    Services.TryAddSingleton(serviceType, implType);
                    break;
                case FeignClientLifetime.Scoped:
                    Services.TryAddScoped(serviceType, implType);
                    break;
                case FeignClientLifetime.Transient:
                    Services.TryAddTransient(serviceType, implType);
                    break;
                default:
                    break;
            }
        }
        public void AddService(Type serviceType, FeignClientLifetime lifetime)
        {
            switch (lifetime)
            {
                case FeignClientLifetime.Singleton:
                    Services.TryAddSingleton(serviceType);
                    break;
                case FeignClientLifetime.Scoped:
                    Services.TryAddScoped(serviceType);
                    break;
                case FeignClientLifetime.Transient:
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

        public void AddOrUpdateService(Type serviceType, Type implType, FeignClientLifetime lifetime)
        {
            RemoveService(serviceType);
            AddService(serviceType, implType, lifetime);
        }
        public void AddOrUpdateService(Type serviceType, FeignClientLifetime lifetime)
        {
            RemoveService(serviceType);
            AddService(serviceType, lifetime);
        }
        public void AddOrUpdateService<TService>(TService service) where TService : class
        {
            RemoveService(typeof(TService));
            AddService(service);
        }

        void RemoveService(Type serviceType)
        {
            var service = Services.FirstOrDefault(a => a.ServiceType == serviceType);
            if (service != null)
            {
                Services.Remove(service);
            }
        }

    }
}
