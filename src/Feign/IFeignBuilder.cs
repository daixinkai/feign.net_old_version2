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

        void AddService(Type serviceType, Type implType, FeignClientScope scope);

        void AddService(Type serviceType, FeignClientScope scope);

        void AddService<TService>(TService service) where TService : class;

        //bool IsRegister(Type serviceType);

    }
}
