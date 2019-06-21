using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Standalone
{
    class ServiceDescriptor
    {

        public ServiceDescriptor(Type serviceType, object instance)
        {
            ServiceType = serviceType;
            ImplementationInstance = instance;
        }

        public ServiceDescriptor(Type serviceType, Type implementationType, FeignClientLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }


        public Type ImplementationType { get; }
        public Type ServiceType { get; }
        public FeignClientLifetime Lifetime { get; }
        public object ImplementationInstance { get; }

        public object GetService()
        {
            if (ImplementationInstance != null)
            {
                return ImplementationInstance;
            }
            switch (Lifetime)
            {
                case FeignClientLifetime.Singleton:
                    object value = Singletons.GetInstance(ServiceType);
                    if (value == null)
                    {
                        //set instance
                        value = GetNewService(ImplementationType);
                        Singletons.SetInstance(ServiceType, value);
                    }
                    return value;
                //case FeignClientLifetime.Scoped:
                case FeignClientLifetime.Transient:
                    return GetNewService(ImplementationType);
                default:
                    return null;
            }
        }


        static object GetNewService(Type type)
        {
            return Activator.CreateInstance(type);
        }

    }
}
