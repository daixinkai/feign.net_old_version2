using Feign.Formatting;
using Feign.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IFeignBuilder
    {
        IFeignOptions Options { get; }
        FeignClientTypeBuilder TypeBuilder { get; }

        void AddService(Type serviceType, Type implType, FeignClientLifetime lifetime);

        void AddService(Type serviceType, FeignClientLifetime lifetime);

        void AddService<TService>(TService service) where TService : class;

        bool HasService(Type serviceType);

        void RemoveService(Type serviceType);

    }
}
